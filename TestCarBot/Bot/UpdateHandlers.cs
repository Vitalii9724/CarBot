using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using TestCarBot.DbService;
using TestCarBot.Models;
using TestCarBot.Services;

namespace TestCarBot.Bot
{
    public class UpdateHandlers
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public UpdateHandlers(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken token)
        {
            if (update.Type != UpdateType.Message || update.Message == null)
                return;

            var message = update.Message;
            var chatId = message.Chat.Id;

            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var commands = scope.ServiceProvider.GetRequiredService<BotCommands>();

            var user = await db.Users
                .Include(u => u.Documents)
                .FirstOrDefaultAsync(u => u.TelegramUserId == chatId, token);

            if (user == null)
            {
                user = new UserData
                {
                    TelegramUserId = chatId,
                    FirstName = message.Chat.FirstName,
                    LastName = message.Chat.LastName,
                    State = UserStateEnum.AwaitingStart
                };
                db.Users.Add(user);
                await db.SaveChangesAsync(token);
            }

            if (message.Text == "/start")
            {
                await commands.HandleStartCommand(bot, user);
                return;
            }

            switch (user.State)
            {
                case UserStateEnum.AwaitingStart:
                case UserStateEnum.Completed:
                    await commands.HandleWrongState(bot, user);
                    break;

                case UserStateEnum.AwaitingPassport:
                case UserStateEnum.AwaitingVehicleDocument:
                    if (message.Photo != null || message.Document != null)
                        await commands.HandleDocumentUpload(bot, message, user);
                    else
                        await commands.HandleFreeFormText(bot, message);
                    break;

                case UserStateEnum.AwaitingDataConfirmation:
                    if (message.Text == BotScenarios.ConfirmDataYes)
                        await commands.HandleConfirmationAccepted(bot, user);
                    else if (message.Text == BotScenarios.ConfirmDataNo)
                        await commands.HandleConfirmationRejected(bot, user);
                    else
                        await commands.HandleFreeFormText(bot, message);
                    break;

                case UserStateEnum.AwaitingPriceAgreement:
                    if (message.Text == BotScenarios.ConfirmPriceYes)
                        await commands.HandlePurchaseConfirmed(bot, user);
                    else if (message.Text == BotScenarios.ConfirmPriceNo)
                        await commands.HandlePurchaseRejected(bot, user);
                    else
                        await commands.HandleFreeFormText(bot, message);
                    break;

                default:
                    await commands.HandleFreeFormText(bot, message);
                    break;
            }
        }

        public Task HandleErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken token)
        {
            var errorMessage = exception switch
            {
                ApiRequestException apiEx => $"Telegram API Error:\n[{apiEx.ErrorCode}] {apiEx.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine($"❌ Error: {errorMessage}");
            return Task.CompletedTask;
        }
    }
}