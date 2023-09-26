using Microsoft.EntityFrameworkCore;
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
using SKitLs.Bots.Telegram.Core.Model.Building;
using SKitLs.Bots.Telegram.Core.Model.Interactions;
using SKitLs.Bots.Telegram.Core.Model.Interactions.Defaults;
using SKitLs.Bots.Telegram.Core.Model.Management.Defaults;
using SKitLs.Bots.Telegram.Core.Model.UpdateHandlers.Defaults;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting.Signed;
using SKitLs.Bots.Telegram.Core.Prototype;
using SKitLs.Bots.Telegram.PageNavs;
using SKitLs.Bots.Telegram.PageNavs.Model;
using SKitLs.Bots.Telegram.Stateful.Model;
using SKitLs.Bots.Telegram.Stateful.Prototype;
using SKitLs.Utils.Localizations.Prototype;
using System.Reflection;
using Telegram.Bot.Requests;
using Telegram.Bot.Types;
using TelegramBot.Dtos;
using TelegramBot.Entities;
using TelegramBot.Extensions;
using TelegramBot.Users;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;


namespace TelegramBot
{
    internal class Program
    {
        private static OracleContext context;

        public static async Task Main(string[] args)
        {
            // Set up configuration
            IConfiguration configuration = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
           .Build();

			// Set up entity framework connection to postgresql
			context = new OracleContext(configuration["ConnectionSetting:DefaultConnection"]);

			context.Database.EnsureCreated();

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

			var inputStateSection = new DefaultStateSection<SignedMessageTextUpdate>();

			inputStateSection.EnableState(InputFullNameState);

			inputStateSection.AddSafely(ExitInput);
			inputStateSection.AddSafely(InputFullName);

			statefulInputs.AddSectionSafely(inputStateSection);

			privateMessages.TextMessageUpdateHandler = privateTexts;

			var mm = GetMenuManager();
			var privateCallbacks = new DefaultCallbackHandler()
			{
				CallbackManager = new DefaultActionManager<SignedCallbackUpdate>(),
			};

			privateCallbacks.CallbackManager.AddSafely(StartSearching);
			privateCallbacks.CallbackManager.AddSafely(NextPage);
			privateCallbacks.CallbackManager.AddSafely(PrevPage);
			privateCallbacks.CallbackManager.AddSafely(SelectUser);

			mm.ApplyTo(privateCallbacks.CallbackManager);

			ChatDesigner privates = ChatDesigner.NewDesigner()
			.UseUsersManager(new UserManager())
			.UseMessageHandler(privateMessages)
			.UseCallbackHandler(privateCallbacks);

			await BotBuilder.NewBuilder(token)
			   .EnablePrivates(privates)
				.AddService<IArgsSerializeService>(new DefaultArgsSerializeService())
				.AddService(mm)
				.CustomDelivery(new AdvancedDeliverySystem())
				.Build()
				.Listen();
		}

		private static DefaultCommand StartCommand => new("start", Do_StartAsync);
		private static async Task Do_StartAsync(SignedMessageTextUpdate update)
		{
			var mm = update.Owner.ResolveService<IMenuManager>();

			// Получаем определённую страницу по id
			// ...StaticPage( { это id -> } "main", "Главная"...
			var page = mm.GetDefined("main");

			await mm.PushPageAsync(page, update);
		}

		private static IOutputMessage GetRequestList(ISignedUpdate? update)
		{
			var message = "Ваши заявки:\n\n";
			if (update is not null && update.Sender is BotUser user)
			{
				message += "Ничего нет";

			}
			return new OutputMessageText(message);
		}

		private static IOutputMessage GetUserData(ISignedUpdate? update)
		{
			var message = "Ваши Данные:\n\n";
			var menu = new PairedInlineMenu();
			if (update is not null && update.Sender is BotUser user)
			{
				message += "Ничего нет";
				menu.Add("Добавить данные", update.Owner.ResolveService<IMenuManager>().OpenPageCallback);
			}

			var res = new OutputMessageText(message)
			{
				Menu = menu
			};

			return res;
		}

		private static IMenuManager GetMenuManager()
		{
			var mm = new DefaultMenuManager();

			var mainBody = new DynamicMessage(u =>
			{
				return new OutputMessageText("Добро пожаловать!\n\nЧего желаете?");
			});

			var mainMenu = new PageNavMenu();
			var mainPage = new WidgetPage("main", "Главная", mainBody, mainMenu);

			var listBody = new DynamicMessage(GetRequestList);

			var listPage = new WidgetPage("saved", "Проверить заявки", listBody);

			var createBody = new DynamicMessage(GetUserData);

			var createPage = new WidgetPage("other", "Мои Данные", createBody);

			mainMenu.PathTo(listPage);
			mainMenu.PathTo(createPage);
			mainMenu.AddAction(StartSearching);

			mm.Define(mainPage);
			mm.Define(listPage);
			mm.Define(createPage);

			return mm;
		}

		public static DefaultUserState DefaultState = new(0, "default");
		public static DefaultUserState InputFullNameState = new(10, "typing");
		public static DefaultUserState SelectState = new(11, "selecting");

		private static DefaultTextInput ExitInput => new("Выйти", Do_ExitInputCityAsync);
		private static async Task Do_ExitInputCityAsync(SignedMessageTextUpdate update)
		{
			// Просто меняем состояние пользователя на исходное
			if (update.Sender is IStatefulUser stateful)
			{
				stateful.State = DefaultState;

				var message = new OutputMessageText($"Вы больше ничего не вводите.")
				{
					Menu = new ReplyCleaner(),
				};
				await update.Owner.DeliveryService.ReplyToSender(message, update);
			}
		}

		private static AnyInput InputFullName => new("fullname", Do_InputFullNameAsync);
		private static async Task Do_InputFullNameAsync(SignedMessageTextUpdate update)
		{
			string fullName = update.Text;
			var message = new OutputMessageText("Вы ввели: " + fullName);

			await update.Owner.DeliveryService.ReplyToSender(message, update);

			var users = context.Ameis.Where(value => EF.Functions.Like(value.Nachn + " " + value.Vorna + " " + value.Midnm, $"%{fullName}%")).ToList();

			if(users.Count > 0)
			{
				var found = new OutputMessageText($"Есть {users.Count} совпадений");
				await update.Owner.DeliveryService.ReplyToSender(found, update);

				var botUser = update.Sender as BotUser;
				botUser.UserPages.Users = users;
				botUser.State = SelectState;

				var menu = new PairedInlineMenu()
				{
					Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
				};

				if (botUser.UserPages.Page * PageCount + PageCount < botUser.UserPages.Users.Count)
					menu.Add("Следующая страница", NextPage);

				int count = botUser.UserPages.Page * PageCount + PageCount;

				if (botUser.UserPages.Page * PageCount + PageCount > botUser.UserPages.Users.Count)
					count = botUser.UserPages.Users.Count;

				var resMsg = $"Результат поиска (страница - {botUser.UserPages.Page + 1}, записей - {count}):\n\n";


				for (int i = botUser.UserPages.Page * PageCount; i < count; i++)
				{
					var user = botUser.UserPages.Users[i];

					resMsg += $"{i + 1}) Пользователь: {user.Nachn + " " + user.Vorna + " " + user.Midnm}\n" +
						   $"Должность - {user.Position}\n\n";
					menu.Add($"{user.Nachn + " " + user.Vorna + " " + user.Midnm}", SelectUser, new IntWrapper(user.Usrid));
				}

				var res = new OutputMessageText(resMsg)
				{
					Menu = menu,
				};
				await update.Owner.DeliveryService.ReplyToSender(res, update);
			}
			else
			{
				var notFound = new OutputMessageText("Пользватель не найден!");
				await update.Owner.DeliveryService.ReplyToSender(notFound, update);
			}
		}

		private static int PageCount = 5;

		private static DefaultCallback NextPage => new("следующая_страница", "Следующая страница", Do_NextPage);
		private static DefaultCallback PrevPage => new("предыдущая_страница", "Предыдущая страница", Do_PrevPage);

		private static async Task Do_PrevPage(SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser sender)
			{
				sender.UserPages.Page--;

				await Page(update);
			}
		}

		private static async Task Do_NextPage(SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser sender)
			{
				sender.UserPages.Page++;

				await Page(update);
			}
		}

		private static async Task Page(SignedCallbackUpdate update)
		{
			BotUser sender = (BotUser) update.Sender;

			if(sender.UserPages.Users is null)
			{
				return;
			}

			var menu = new PairedInlineMenu()
			{
				Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
			};

			int count = sender.UserPages.Page * PageCount + PageCount;

			if (sender.UserPages.Page * PageCount + PageCount > sender.UserPages.Users.Count)
				count = sender.UserPages.Users.Count;

			if (sender.UserPages.Page * PageCount + PageCount < sender.UserPages.Users.Count)
				menu.Add("Следующая страница", NextPage);
			if (sender.UserPages.Page > 0)
				menu.Add("Предыдущая страница", PrevPage);

			var resMsg = $"Результат поиска (страница - {sender.UserPages.Page + 1}, записей - {count - sender.UserPages.Page * PageCount}):\n\n";

			for (int i = sender.UserPages.Page * PageCount; i < count; i++)
			{
				var user = sender.UserPages.Users[i];

				resMsg += $"{i + 1}) Пользователь: {user.Nachn + " " + user.Vorna + " " + user.Midnm}\n" +
					   $"Должность - {user.Position}\n\n";
				menu.Add($"{user.Nachn + " " + user.Vorna + " " + user.Midnm}", SelectUser, new IntWrapper(user.Usrid));
			}

			var res = new OutputMessageText(resMsg)
			{
				Menu = menu,
			};

			await update.Owner.DeliveryService.ReplyToSender(new EditWrapper(res, update.TriggerMessageId), update);
		}

		private static BotArgedCallback<IntWrapper> SelectUser => new(new LabeledData("Выбор", "SelectUser"), Do_SelectUserAsync);
		private static async Task Do_SelectUserAsync(IntWrapper args, SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser user)
			{
				user.State = DefaultState;
				user.UserPages.Page = 0;

				var amei = context.Ameis.Where(User => User.Usrid == args.Value).First();
				var message = new OutputMessageText(update.Message.Text + $"\n\nВы выбрали {amei.Nachn + " " + amei.Vorna + " " + amei.Midnm}")
				{
					Menu = null,
				};
				await update.Owner.DeliveryService.ReplyToSender(new EditWrapper(message, update.TriggerMessageId), update);
			}
		}

		private static DefaultCallback StartCreating => new("createRequest", "Создать заявку", Do_CreateAsync);
		private static async Task Do_CreateAsync(SignedCallbackUpdate update)
		{
			if (update.Sender is IStatefulUser sender)
			{
			}
		}

		private static DefaultCallback StartSearching => new("startCreation", "Создать заявку", Do_SearchAsync);
		private static async Task Do_SearchAsync(SignedCallbackUpdate update) 
		{
			if (update.Sender is IStatefulUser sender)
			{
				sender.State = InputFullNameState;
				await update.Owner.DeliveryService.ReplyToSender("Введите ФИО или \"Выйти\"", update);
			}

		}
	}
}