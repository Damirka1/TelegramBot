using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SKitLs.Bots.Telegram.AdvancedMessages.AdvancedDelivery;
using SKitLs.Bots.Telegram.AdvancedMessages.Model;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Menus;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Messages;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Messages.Text;
using SKitLs.Bots.Telegram.AdvancedMessages.Prototype;
using SKitLs.Bots.Telegram.ArgedInteractions.Argumentation;
using SKitLs.Bots.Telegram.ArgedInteractions.Interactions.Model;
using SKitLs.Bots.Telegram.BotProcesses.Model;
using SKitLs.Bots.Telegram.BotProcesses.Prototype;
using SKitLs.Bots.Telegram.Core.Model.Building;
using SKitLs.Bots.Telegram.Core.Model.Interactions;
using SKitLs.Bots.Telegram.Core.Model.Interactions.Defaults;
using SKitLs.Bots.Telegram.Core.Model.Management.Defaults;
using SKitLs.Bots.Telegram.Core.Model.UpdateHandlers.Defaults;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting.Signed;
using SKitLs.Bots.Telegram.Core.Prototype;
using SKitLs.Bots.Telegram.DataBases;
using SKitLs.Bots.Telegram.DataBases.Model.Datasets;
using SKitLs.Bots.Telegram.PageNavs;
using SKitLs.Bots.Telegram.PageNavs.Model;
using SKitLs.Bots.Telegram.PageNavs.Prototype;
using SKitLs.Bots.Telegram.Stateful.Model;
using SKitLs.Bots.Telegram.Stateful.Prototype;
using SKitLs.Utils.Localizations.Prototype;
using System.Reflection;
using Telegram.Bot;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Dtos;
using TelegramBot.Entities;
using TelegramBot.Extensions;
using TelegramBot.Services;
using TelegramBot.Users;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static TelegramBot.Entities.PassStatus;

namespace TelegramBot
{
    internal class Program
    {
        private static OracleContext context;

		private static BotService botService;

		private static RequestService requestService;

		private static CalendarService calendarService;

        public static async Task Main(string[] args)
        {
            // Set up configuration
            IConfiguration configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

			// Set up entity framework connection to postgresql
			context = new OracleContext(configuration["ConnectionSetting:DefaultConnection"]);

			context.Database.EnsureCreated();
			requestService = new RequestService(context);

			calendarService = new CalendarService();

			botService = new BotService(context, requestService, calendarService);

			Console.WriteLine("Start Listening");

			// Next set up TelegramBot framework
			await PrepareBot(configuration["TelegramBotToken"]);
        }

        public static async Task PrepareBot(string token)
        {
			BotBuilder.DebugSettings.DebugLanguage = LangKey.RU;
			BotBuilder.DebugSettings.UpdateLocalsPath("resources/locals");

			var privateMessages = new DefaultSignedMessageUpdateHandler();

			var statefulInputs = new StatefulActionManager<SignedMessageTextUpdate>();

			var privateTexts = new DefaultSignedMessageTextUpdateHandler
			{
				CommandsManager = new DefaultActionManager<SignedMessageTextUpdate>(),
				TextInputManager = statefulInputs,
			};
			privateTexts.CommandsManager.AddSafely(StartCommand);
			privateTexts.CommandsManager.AddSafely(MenuCommand);
			privateTexts.CommandsManager.AddSafely(PhoneCommand);

			botService.SetUpStates(statefulInputs);

			var other = new DefaultSignedMessageUpdateHandler {
				RestMessagesUpdateHandler = new RestMessageHandler(botService)
			};

			privateMessages.TextMessageUpdateHandler = privateTexts;
			privateMessages.RestMessagesUpdateHandler = other;

			var mm = botService.GetMenuManager();

			var statefulCallbacks = new StatefulActionManager<SignedCallbackUpdate>();

			var privateCallbacks = new DefaultCallbackHandler()
			{
				CallbackManager = statefulCallbacks,
			};

			botService.SetUpCallBacks(privateCallbacks.CallbackManager);

			mm.ApplyTo(privateCallbacks.CallbackManager);

			ChatDesigner privates = ChatDesigner.NewDesigner()
			.UseUsersManager(new UserManager())
			.UseMessageHandler(privateMessages)
			.UseCallbackHandler(privateCallbacks);

			var bot = BotBuilder.NewBuilder(token)
				.CustomDelivery(new AdvancedDeliverySystem())
				.EnablePrivates(privates)
				.AddService<IArgsSerializeService>(new DefaultArgsSerializeService())
				.AddService(mm)
				.AddService<IProcessManager>(new DefaultProcessManager())
				.Build();

			bot.Settings.BotLanguage = LangKey.RU;

			await bot.Listen();
		}

		private static DefaultCommand StartCommand => new("start", Do_StartAsync);

		private static DefaultCommand MenuCommand => new("menu", Do_StartAsync);
		private static async Task Do_StartAsync(SignedMessageTextUpdate update)
		{
			var mm = update.Owner.ResolveService<IMenuManager>();

			// Получаем определённую страницу по id
			// ...StaticPage( { это id -> } "main", "Главная"...

			var user = update.Sender as BotUser;

			user.UserPages.Page = 0;

			var page = mm.GetDefined("main");

			await mm.PushPageAsync(page, update, true);
		}

		private static DefaultCommand PhoneCommand => new("phone", Do_GetPhoneAsync);
		private static async Task Do_GetPhoneAsync(SignedMessageTextUpdate update)
		{

			ReplyKeyboardMarkup requestReplyKeyboard = new(
			new[]
			{
				KeyboardButton.WithRequestContact("Отправить номер телефона")
			});

			await update.Owner.Bot.SendTextMessageAsync(chatId: update.ChatId, text: "Отправьте ваш телефон для авторизации", replyMarkup: requestReplyKeyboard);
		}

	}
}