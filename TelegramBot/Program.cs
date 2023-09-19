using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SKitLs.Bots.Telegram.AdvancedMessages.AdvancedDelivery;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Messages;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Messages.Text;
using SKitLs.Bots.Telegram.ArgedInteractions.Argumentation;
using SKitLs.Bots.Telegram.Core.Model.Building;
using SKitLs.Bots.Telegram.Core.Model.Interactions.Defaults;
using SKitLs.Bots.Telegram.Core.Model.Management.Defaults;
using SKitLs.Bots.Telegram.Core.Model.UpdateHandlers.Defaults;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting.Signed;
using SKitLs.Bots.Telegram.PageNavs.Model;
using SKitLs.Bots.Telegram.PageNavs.Prototype;
using SKitLs.Bots.Telegram.Stateful.Model;
using SKitLs.Bots.Telegram.Stateful.Prototype;
using SKitLs.Utils.Localizations.Prototype;
using TelegramBot.Users;


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
            //context = new OracleContext(configuration["ConnectionSetting:DefaultConnection"]);

            //context.Database.EnsureCreated();

			Console.WriteLine("Start Listening");

			// Next set up TelegramBot framework
			await PrepareBot(configuration["TelegramBotToken"]);
        }

        public static async Task PrepareBot(string token)
        {
			BotBuilder.DebugSettings.DebugLanguage = LangKey.RU;
			BotBuilder.DebugSettings.UpdateLocalsPath("resources/locals");

			var privateMessages = new DefaultSignedMessageUpdateHandler();
			var privateTexts = new DefaultSignedMessageTextUpdateHandler
			{
				CommandsManager = new DefaultActionManager<SignedMessageTextUpdate>()
			};
			privateTexts.CommandsManager.AddSafely(StartCommand);
			privateMessages.TextMessageUpdateHandler = privateTexts;

			var mm = GetMenuManager();
			var privateCallbacks = new DefaultCallbackHandler()
			{
				CallbackManager = new DefaultActionManager<SignedCallbackUpdate>(),
			};

			privateCallbacks.CallbackManager.AddSafely(StartSearching);

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

		private static IMenuManager GetMenuManager()
		{
			var mm = new DefaultMenuManager();

			var mainBody = new OutputMessageText("Добро пожаловать!\n\nЧего желаете?");
			var mainMenu = new PageNavMenu();
			var mainPage = new WidgetPage("main", "Главная", mainBody, mainMenu);

			var listBody = new OutputMessageText("Здесь будут отображаться заявки...");

			var listPage = new WidgetPage("saved", "Проверить заявки", listBody);

			var createBody = new OutputMessageText("Здесь будут отображаться данные пользователя");

			var createPage = new WidgetPage("other", "Мои Данные", createBody);

			mainMenu.PathTo(listPage);
			mainMenu.PathTo(createPage);
			mainMenu.AddAction(StartSearching);

			mm.Define(mainPage);
			mm.Define(listPage);
			mm.Define(createPage);

			return mm;
		}

		// Этот коллбэк будет вызывать поиск. Пока что прототип.
		private static DefaultCallback StartSearching => new("startCreation", "Создать заявку", Do_SearchAsync);
		private static async Task Do_SearchAsync(SignedCallbackUpdate update) 
		{
			if (update.Sender is IStatefulUser sender)
			{
				int st = sender.State.StateId;

				if(st == 0)
				{
					await update.Owner.DeliveryService.AnswerSenderAsync("Введите ФИО", update);
					sender.State = new DefaultUserState(st + 1);
				}
				else
				{
					await update.Owner.DeliveryService.AnswerSenderAsync("Вы ещё не ввели ФИО сотрудника", update);
				}
			}

		}
	}
}