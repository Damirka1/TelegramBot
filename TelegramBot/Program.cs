﻿using Microsoft.EntityFrameworkCore;
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
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
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

			var statefulInputs = new DefaultStatefulManager<SignedMessageTextUpdate>();

			var privateTexts = new DefaultSignedMessageTextUpdateHandler
			{
				CommandsManager = new DefaultActionManager<SignedMessageTextUpdate>(),
				TextInputManager = statefulInputs,
			};
			privateTexts.CommandsManager.AddSafely(StartCommand);
			privateTexts.CommandsManager.AddSafely(MenuCommand);

			botService.SetUpStates(statefulInputs);

			privateMessages.TextMessageUpdateHandler = privateTexts;

			var mm = botService.GetMenuManager();

			var statefulCallbacks = new DefaultStatefulManager<SignedCallbackUpdate>();

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
			   .EnablePrivates(privates)
				.AddService<IArgsSerializeService>(new DefaultArgsSerializeService())
				.AddService(mm)
				.AddService<IProcessManager>(new DefaultProcessManager())
				.CustomDelivery(new AdvancedDeliverySystem())
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
			var page = mm.GetDefined("main");

			await mm.PushPageAsync(page, update, true);
		}

	}
}