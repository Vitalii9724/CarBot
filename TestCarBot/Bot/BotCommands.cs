using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TestCarBot.DbService;
using TestCarBot.Models;
using TestCarBot.Services;

namespace TestCarBot.Bot
{
    public class BotCommands
    {
        private readonly AppDbContext _db;
        private readonly OcrService _ocrService;
        private readonly OpenAiService _openAi;
        private readonly PdfGeneratorService _pdf;

        public BotCommands(AppDbContext db, OcrService ocrService, PdfGeneratorService pdf, OpenAiService openAi)
        {
            _db = db;
            _ocrService = ocrService;
            _pdf = pdf;
            _openAi = openAi;
        }

        public async Task HandleStartCommand(ITelegramBotClient bot, UserData user)
        {
            var oldDocs = await _db.Documents.Where(d => d.TelegramUserId == user.TelegramUserId).ToListAsync();
            if (oldDocs.Any())
            {
                _db.Documents.RemoveRange(oldDocs);
            }

            var welcomeMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.Welcome);
            await bot.SendTextMessageAsync(user.TelegramUserId, welcomeMessage, replyMarkup: new ReplyKeyboardRemove());

            var requestPassportMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.RequestPassport);
            await bot.SendTextMessageAsync(user.TelegramUserId, requestPassportMessage);

            user.State = UserStateEnum.AwaitingPassport;
            await SaveChangesWithHandling(bot, user.TelegramUserId);
        }

        public async Task HandleDocumentUpload(ITelegramBotClient bot, Message message, UserData user)
        {
            var docType = user.State == UserStateEnum.AwaitingPassport ? DocumentTypeEnum.Passport : DocumentTypeEnum.Vehicle;
            var extractedData = await _ocrService.ExtractDataAsync(docType);

            Console.WriteLine($"\n--- 📝 MOCK OCR Result for {docType} (User: {user.TelegramUserId}) ---");
            Console.WriteLine($"  - Last Name:  {extractedData.LastName ?? "Not found"}");
            Console.WriteLine($"  - First Name: {extractedData.FirstName ?? "Not found"}");
            Console.WriteLine($"  - Passport:   {extractedData.PassportNumber ?? "Not found"}");
            Console.WriteLine($"  - Car Model:  {extractedData.CarModel ?? "Not found"}");
            Console.WriteLine($"  - Car Number: {extractedData.CarNumber ?? "Not found"}");
            Console.WriteLine("----------------------------------------------------------\n");

            if (docType == DocumentTypeEnum.Passport)
            {
                user.FirstName = extractedData.FirstName;
                user.LastName = extractedData.LastName;
                user.PassportNumber = extractedData.PassportNumber;
            }
            else if (docType == DocumentTypeEnum.Vehicle)
            {
                user.CarNumber = extractedData.CarNumber;
                user.CarModel = extractedData.CarModel;
            }

            if (docType == DocumentTypeEnum.Passport)
            {
                var passportReceivedMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.PassportReceived);
                await bot.SendTextMessageAsync(user.TelegramUserId, passportReceivedMessage);
                user.State = UserStateEnum.AwaitingVehicleDocument;
            }
            else
            {
                var documentsReceivedMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.DocumentsReceived);
                await bot.SendTextMessageAsync(user.TelegramUserId, documentsReceivedMessage);
                await ProcessDocumentsAndAskForConfirmation(bot, user);
            }

            await SaveChangesWithHandling(bot, user.TelegramUserId);
        }

        private async Task ProcessDocumentsAndAskForConfirmation(ITelegramBotClient bot, UserData user)
        {
            var confirmationMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.ConfirmData, new Dictionary<string, string>
            {
                { "last_name", user.LastName ?? "не розпізнано" },
                { "first_name", user.FirstName ?? "не розпізнано" },
                { "passport", user.PassportNumber ?? "не розпізнано" },
                { "car_model", user.CarModel ?? "не розпізнано" },
                { "car_number", user.CarNumber ?? "не розпізнано" }
            });

            var buttons = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton(BotScenarios.ConfirmDataYes), new KeyboardButton(BotScenarios.ConfirmDataNo) }
            })
            { ResizeKeyboard = true, OneTimeKeyboard = true };

            await bot.SendTextMessageAsync(user.TelegramUserId, confirmationMessage, replyMarkup: buttons);
            user.State = UserStateEnum.AwaitingDataConfirmation;
        }

        public async Task HandleConfirmationAccepted(ITelegramBotClient bot, UserData user)
        {
            var priceMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.DataAccepted);

            var buttons = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton(BotScenarios.ConfirmPriceYes), new KeyboardButton(BotScenarios.ConfirmPriceNo) }
            })
            { ResizeKeyboard = true, OneTimeKeyboard = true };

            await bot.SendTextMessageAsync(user.TelegramUserId, priceMessage, replyMarkup: buttons);
            user.State = UserStateEnum.AwaitingPriceAgreement;
            await SaveChangesWithHandling(bot, user.TelegramUserId);
        }

        public async Task HandleConfirmationRejected(ITelegramBotClient bot, UserData user)
        {
            var userDocs = await _db.Documents.Where(d => d.TelegramUserId == user.TelegramUserId).ToListAsync();
            if (userDocs.Any())
            {
                _db.Documents.RemoveRange(userDocs);
            }
            user.FirstName = null;
            user.LastName = null;
            user.PassportNumber = null;
            user.CarNumber = null;
            user.CarModel = null;

            var rejectionMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.DataRejected);
            await bot.SendTextMessageAsync(user.TelegramUserId, rejectionMessage, replyMarkup: new ReplyKeyboardRemove());

            user.State = UserStateEnum.AwaitingPassport;
            await SaveChangesWithHandling(bot, user.TelegramUserId);
        }

        public async Task HandlePurchaseConfirmed(ITelegramBotClient bot, UserData user)
        {
            var policyNumber = $"POL-{Guid.NewGuid().ToString()[..8].ToUpper()}";
            var issuedAt = DateTime.UtcNow;

            var newPolicy = new InsurancePolicy
            {
                TelegramUserId = user.TelegramUserId,
                PolicyNumber = policyNumber,
                IssuedAt = issuedAt
            };
            _db.Policies.Add(newPolicy);

            var pdfBytes = await _pdf.GeneratePolicyPdfAsync(user, policyNumber, issuedAt);
            using var stream = new MemoryStream(pdfBytes);

            var purchaseMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.PurchaseConfirmed);
            await bot.SendTextMessageAsync(user.TelegramUserId, purchaseMessage, replyMarkup: new ReplyKeyboardRemove());

            await bot.SendDocumentAsync(
                user.TelegramUserId,
                new InputFileStream(stream, $"Policy_{policyNumber}.pdf"),
                caption: "📄 Ваш страховий поліс"
            );

            var finalMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.FinalWords);
            await bot.SendTextMessageAsync(user.TelegramUserId, finalMessage);

            user.State = UserStateEnum.Completed;
            await SaveChangesWithHandling(bot, user.TelegramUserId);
        }

        public async Task HandlePurchaseRejected(ITelegramBotClient bot, UserData user)
        {
            var rejectionMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.PriceRejected);
            await bot.SendTextMessageAsync(user.TelegramUserId, rejectionMessage, replyMarkup: new ReplyKeyboardRemove());

            user.State = UserStateEnum.AwaitingStart;
            await SaveChangesWithHandling(bot, user.TelegramUserId);
        }

        public async Task HandleFreeFormText(ITelegramBotClient bot, Message message)
        {
            var reply = await _openAi.GenerateUserReplyAsync(message.Text!);
            await bot.SendTextMessageAsync(message.Chat.Id, reply);
        }

        public async Task HandleWrongState(ITelegramBotClient bot, UserData user)
        {
            string scenario = user.State switch
            {
                UserStateEnum.AwaitingStart => BotScenarios.AwaitingStart,
                UserStateEnum.Completed => BotScenarios.AlreadyCompleted,
                _ => BotScenarios.GenericError,
            };
            var message = await _openAi.GenerateBotResponseAsync(scenario);
            await bot.SendTextMessageAsync(user.TelegramUserId, message, replyMarkup: new ReplyKeyboardRemove());
        }

        private async Task SaveChangesWithHandling(ITelegramBotClient bot, long userId)
        {
            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"❌ DB Error saving state for user {userId}: {ex.Message}");
                var errorMessage = await _openAi.GenerateBotResponseAsync(BotScenarios.GenericError);
                await bot.SendTextMessageAsync(userId, errorMessage);
            }
        }
    }
}