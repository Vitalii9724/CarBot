
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Microsoft.Extensions.Options;
using TestCarBot.Models;

namespace TestCarBot.Bot
{
    public class BotService
    {
        private readonly TelegramBotClient _botClient;
        private readonly UpdateHandlers _updateHandlers;

        public BotService(IOptions<BotConfiguration> config, UpdateHandlers updateHandlers)
        {
            _botClient = new TelegramBotClient(config.Value.Token);
            _updateHandlers = updateHandlers;
        }

        public async Task StartAsync()
        {
            var me = await _botClient.GetMeAsync();
            Console.WriteLine($"🤖 Бот запущено: @{me.Username}");

            var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // отримуємо всі типи
            };

            _botClient.StartReceiving(
                updateHandler: async (bot, update, token) =>
                {
                    await _updateHandlers.HandleUpdateAsync(bot, update, token);
                },
                errorHandler: async (bot, exception, token) =>
                {
                    await _updateHandlers.HandleErrorAsync(bot, exception, token);
                },
                receiverOptions: receiverOptions,
                cancellationToken: cts.Token
            );

            await Task.Delay(-1, cts.Token);
        }
    }
}
