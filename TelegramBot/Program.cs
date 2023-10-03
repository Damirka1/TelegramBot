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
using static TelegramBot.Entities.PassStatus;

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
			privateTexts.CommandsManager.AddSafely(MenuCommand);

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
			privateCallbacks.CallbackManager.AddSafely(CancelPage);
			privateCallbacks.CallbackManager.AddSafely(SelectTime);
			

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

		private static DefaultCommand MenuCommand => new("menu", Do_StartAsync);
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
				PassUser passUser = null;
				if(context.PassUser.Where(User => User.TelegramId == user.TelegramId).Count() == 0)
				{
					message += "Пользователь не найден!";
				} 
				else
				{
					passUser = context.PassUser.Where(User => User.TelegramId == user.TelegramId).First();
				}

				if (passUser != null) 
				{
					var select = context.PassRequest.Where(PassRequest => PassRequest.From == passUser);

					if(select.Count() > 0)
					{
						int index = 0;
						select.Include(s => s.To)
							.Include(s => s.PassSchedule)
							.Include(s => s.PassStatus)
							.ForEachAsync(value =>
						{
							var req = value;
							message += $"{index + 1}) Заявка от {req.Created:G}\n" +
							$"К кому - {req.To.Nachn + " " + req.To.Vorna + " " + req.To.Midnm}\n";

							var st = req.PassStatus.Last();

							message += "Текущий статус - ";

							switch (st.Status)
							{
								case StatusEnum.Created: message += "Создано"; break;
								case StatusEnum.InProgress: message += "В обработке"; break;
								case StatusEnum.Declined: message += "Отклонено"; break;
								case StatusEnum.Accepted: message += "Создано"; break;
							};

							message += "\n";

							message += $"Дата посещения - {req.PassSchedule.Start:G}\n";
							message += $"Длительность посещения - {req.PassSchedule.End.Subtract(req.PassSchedule.Start).TotalMinutes} минут\n\n";

							index++;
						});
					}
					else 
					{
						message += "Заявок нет!";
					}
				}

				
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
		public static DefaultUserState SelectUserState = new(11, "selecting");
		public static DefaultUserState SelectTimeState = new(12, "selecting");

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
				botUser.State = SelectUserState;

				var menu = new PairedInlineMenu()
				{
					Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
				};

				if (botUser.UserPages.Page * PageCount + PageCount < botUser.UserPages.Users.Count)
					menu.Add("Следующая страница", NextPage);

				menu.Add("Отмена заявки", CancelPage);

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

		private static DefaultCallback CancelPage => new("отмена", "Отмена", Do_CancelPage);

		private static async Task Do_CancelPage(SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser sender)
			{
				sender.State = DefaultState;
				sender.UserPages.Page = 0;

				var message = new OutputMessageText(update.Message.Text + $"\n\nВы отменили заявку")
				{
					Menu = null,
				};

				await update.Owner.DeliveryService.ReplyToSender(new EditWrapper(message, update.TriggerMessageId), update);
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
			if (sender.UserPages.Page > 1)
				menu.Add("Предыдущая страница", PrevPage);

			menu.Add("Отмена заявки", CancelPage);

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

				user.SelectedUser = amei;

				var message = new OutputMessageText(update.Message.Text + $"\n\nВы выбрали {amei.Nachn + " " + amei.Vorna + " " + amei.Midnm}")
				{
					Menu = null,
				};
				await update.Owner.DeliveryService.ReplyToSender(new EditWrapper(message, update.TriggerMessageId), update);

				await Do_ShowRequestMenu(update);
			}
		}

		private static BotArgedCallback<DateWrapper> SelectTime => new(new LabeledData("ВыборВремени", "SelectTime"), Do_SelectTimeAsync);
		private static async Task Do_SelectTimeAsync(DateWrapper args, SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser user)
			{
				var message = new OutputMessageText(update.Message.Text + $"\n\nВы выбрали {args.Value}")
				{
					Menu = null,
				};
				await update.Owner.DeliveryService.ReplyToSender(new EditWrapper(message, update.TriggerMessageId), update);

				try
				{
					await CreateRequest(user, DateTime.Parse(args.Value));

					message = new OutputMessageText(update.Message.Text + $"\n\nЗаявка успешно создана!")
					{
						Menu = null,
					};
					
				} catch(Exception ex)
				{
					message = new OutputMessageText(update.Message.Text + $"\n\nНе удалось создать заявку :(")
					{
						Menu = null,
					};
				}
				await update.Owner.DeliveryService.ReplyToSender(new EditWrapper(message, update.TriggerMessageId), update);
			}
		}

		private static async Task CreateRequest(BotUser botUser, DateTime time)
		{
			if (botUser.SelectedUser == null)
				throw new ArgumentNullException(nameof(botUser.SelectedUser));

			PassUser? passUser = null;

			if (context.PassUser.Where(user => user.TelegramId == botUser.TelegramId).Count() == 0)
			{
				passUser = new PassUser();
				passUser.TelegramId = (int)botUser.TelegramId;

				context.PassUser.Add(passUser);
				context.SaveChanges();
			} 
			else
			{
				passUser = context.PassUser.Where(user => user.TelegramId == botUser.TelegramId).First();
			}

			var now = DateTime.Now;

			PassRequest passRequest = new PassRequest();

			passRequest.Created = now;

			passRequest.From = passUser;

			passRequest.To = botUser.SelectedUser;

			var passStatus = new PassStatus();

			passStatus.Created = now;

			passRequest.PassStatus = new List<PassStatus> { passStatus };

			var passSchedule = new PassSchedule();
			passSchedule.Day = time.Date;
			passSchedule.Start = time;
			passSchedule.End = time.AddHours(1);

			passRequest.PassSchedule = passSchedule;

			context.PassRequest.Add(passRequest);
			context.PassStatus.Add(passStatus);
			context.PassSchedule.Add(passSchedule);

			await context.SaveChangesAsync();
		}

		//private static AnyInput InputTime => new("selectTime", Do_InputTimeAsync);

		//private static async Task Do_InputTimeAsync(SignedMessageTextUpdate update)
		//{

		//}

		private static async Task Do_ShowRequestMenu(SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser user)
			{
				user.State = SelectTimeState;

				var menu = new PairedInlineMenu()
				{
					Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
				};

				var date = DateTime.Now;

				var now = date.AddHours(1);
				var tommorow = now.AddDays(1);

				menu.Add($"Сегодня - {now:G}", SelectTime, new DateWrapper(now.ToString("G")));

				menu.Add($"Завтра - {tommorow:G}", SelectTime, new DateWrapper(tommorow.ToString("G")));

				var message = new OutputMessageText("Выберите дату посещения")
				{
					Menu = menu,
				};
				await update.Owner.DeliveryService.ReplyToSender(message, update);
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