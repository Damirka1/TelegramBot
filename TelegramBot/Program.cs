using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types;

namespace TelegramBot
{
    internal class Program
    {
        private static TelegramBotClient botClient;
        private static OracleContext context;

        public static async Task PrepareBotMenu()
        {
            List<BotCommand> commands = new List<BotCommand>();

            var kzt = new BotCommand();
            kzt.Command = "/kzt";
            kzt.Description = "Вывод курса RUB - KZT";

            commands.Add(kzt);

            await botClient.SetMyCommandsAsync(commands, new BotCommandScopeDefault());
        }

        public static Task Main(string[] args)
        {
            // Set up configuration
            IConfiguration configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

            botClient = new TelegramBotClient(configuration["TelegramBotToken"]);

            // Set up entity framework connection to postgresql
            context = new OracleContext(configuration["ConnectionSetting:DefaultConnection"]);

            context.Database.EnsureCreated();

            Console.WriteLine("Count = " + context.Ameis.Count());

            // Next set up TelegramBot framework

            using var cts = new CancellationTokenSource();

            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
            };

            botClient.StartReceiving(
                HandleUpdateAsync,
                HandlePollingErrorAsync,
                receiverOptions,
                cts.Token);

            PrepareBotMenu();

            Console.WriteLine($"Start listening");
            Console.ReadLine();

            // Send cancellation request to stop bot
            cts.Cancel();
            return Task.CompletedTask;
        }

        private static async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
        {
            // Only process Message updates: https://core.telegram.org/bots/api#message

            var message = update.Message;
            if (update.Message == null)
                return;

            //if (update.Message is not { } message)
            //    return;
            //// Only process text messages
            //if (message.Text is not { "" } messageText)
            //    return;

            if (message.Text == "/kzt")
            {
                double kzt = 0;

                string text = "Привет! Сегодняшний курс на пару RUB KZT 1 к " + string.Format("{0:0.00}", kzt);

                await botClient.SendTextMessageAsync(message.Chat.Id, text);
            }

            Console.WriteLine($"Received a '{message.Text}' message in chat {message.Chat.Id}.");
        }

        private static Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };

            Console.WriteLine(ErrorMessage);
            return Task.CompletedTask;
        }
    }
}