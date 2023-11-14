using Microsoft.EntityFrameworkCore;
using SKitLs.Bots.Telegram.AdvancedMessages.Model;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Menus;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Messages;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Messages.Text;
using SKitLs.Bots.Telegram.AdvancedMessages.Prototype;
using SKitLs.Bots.Telegram.ArgedInteractions.Argumentation;
using SKitLs.Bots.Telegram.ArgedInteractions.Interactions.Model;
using SKitLs.Bots.Telegram.Core.Model.Interactions;
using SKitLs.Bots.Telegram.Core.Model.Interactions.Defaults;
using SKitLs.Bots.Telegram.Core.Model.Management;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting.Signed;
using SKitLs.Bots.Telegram.PageNavs;
using SKitLs.Bots.Telegram.PageNavs.Model;
using SKitLs.Bots.Telegram.Stateful.Model;
using SKitLs.Bots.Telegram.Stateful.Prototype;
using System.Globalization;
using Telegram.Bot;
using TelegramBot.Dtos;
using TelegramBot.Entities;
using TelegramBot.Extensions;
using TelegramBot.Services.Calendar;
using TelegramBot.Users;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static TelegramBot.Entities.PassStatus;

namespace TelegramBot.Services
{
	internal class BotService
	{
		private DefaultUserState DefaultState = new(0, "default");
		private DefaultUserState InputUserFullNameState = new(5, "typing");
		private DefaultUserState InputIINState = new(6, "typing");
		private DefaultUserState InputTelephoneState = new(7, "typing");
		private DefaultUserState InputAmeiState = new(8, "typing");
		private DefaultUserState InputFullNameState = new(10, "typing");
		private DefaultUserState SelectUserState = new(11, "selecting");
		private DefaultUserState InputReasonState = new(12, "selecting");
		private DefaultUserState SelectTimeState = new(13, "selecting");

		private OracleContext context;
		private RequestService requestService;
		private CalendarService calendarService;

		public BotService(OracleContext context, RequestService requestService, CalendarService calendarService) 
		{
			this.context = context;
			this.requestService = requestService;
			this.calendarService = calendarService;
			calendarService.SetFinishFunc(Do_CreateRequest);
		}

		public void SetUpStates(DefaultStatefulManager<SignedMessageTextUpdate> statefulInputs)
		{
			var inputStateSection = new DefaultStateSection<SignedMessageTextUpdate>();

			inputStateSection.EnableState(InputFullNameState);

			inputStateSection.AddSafely(ExitInput);
			inputStateSection.AddSafely(InputFullName);

			statefulInputs.AddSectionSafely(inputStateSection);

			var inputReasonStateSection = new DefaultStateSection<SignedMessageTextUpdate>();

			inputReasonStateSection.EnableState(InputReasonState);

			inputReasonStateSection.AddSafely(ExitInput);
			inputReasonStateSection.AddSafely(InputReason);

			statefulInputs.AddSectionSafely(inputReasonStateSection);

			var inputUserFullnameSection = new DefaultStateSection<SignedMessageTextUpdate>();

			inputUserFullnameSection.EnableState(InputUserFullNameState);

			inputUserFullnameSection.AddSafely(ExitInput);
			inputUserFullnameSection.AddSafely(InputUserFullname);

			statefulInputs.AddSectionSafely(inputUserFullnameSection);

			var inputUserIINSection = new DefaultStateSection<SignedMessageTextUpdate>();

			inputUserIINSection.EnableState(InputIINState);

			inputUserIINSection.AddSafely(ExitInput);
			inputUserIINSection.AddSafely(InputUserIIN);

			statefulInputs.AddSectionSafely(inputUserIINSection);

			var inputUserTelephoneSection = new DefaultStateSection<SignedMessageTextUpdate>();

			inputUserTelephoneSection.EnableState(InputTelephoneState);

			inputUserTelephoneSection.AddSafely(ExitInput);
			inputUserTelephoneSection.AddSafely(InputUserTelephone);

			statefulInputs.AddSectionSafely(inputUserTelephoneSection);

			var inputUserAmeiSection = new DefaultStateSection<SignedMessageTextUpdate>();

			inputUserAmeiSection.EnableState(InputAmeiState);

			inputUserAmeiSection.AddSafely(ExitInput);
			inputUserAmeiSection.AddSafely(InputUserAmei);

			statefulInputs.AddSectionSafely(inputUserAmeiSection);
		}

		public void SetUpCallBacks(IActionManager<SignedCallbackUpdate> callbackManager)
		{
			callbackManager.AddSafely(StartSearching);
			callbackManager.AddSafely(NextPage);
			callbackManager.AddSafely(PrevPage);
			callbackManager.AddSafely(SelectUser);
			callbackManager.AddSafely(CancelPage);
			callbackManager.AddSafely(StartDataEntering);
			callbackManager.AddSafely(StartAmeiEntering);
			callbackManager.AddSafely(calendarService.CalendarCallBack);
		}

		private async Task Do_ShowRequestMenu(ISignedUpdate update)
		{
			if (update.Sender is BotUser user)
			{
				user.State = SelectTimeState;

				calendarService.ShowCalendar(update);
			}
		}

		private IOutputMessage GetRequestList(ISignedUpdate? update)
		{
			var message = "Ваши заявки:\n\n";
			if (update is not null && update.Sender is BotUser user)
			{
				PassUser passUser = null;
				if (context.PassUser.Where(User => User.TelegramId == user.TelegramId).Count() == 0)
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

					if (select.Count() > 0)
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
									case StatusEnum.Accepted: message += "Принято"; break;
									case StatusEnum.Declined: message += "Отклонено"; break;
									case StatusEnum.Closed: message += "Завершено"; break;
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

		private IOutputMessage GetUserData(ISignedUpdate? update)
		{
			var message = "Ваши Данные:\n\n";

			if (update is not null && update.Sender is BotUser botUser)
			{
				PassUser? user = context.PassUser.Where(user => user.TelegramId == botUser.TelegramId).FirstOrDefault();

				if (user != null && user.IIN != null)
				{
					message += $"ФИО: {user.Fullname}\n\n";
					message += $"ИИН: {user.IIN}\n\n";
					message += $"Телефон: {user.Telephone}\n\n";
				}
				else
				{
					message += "Ничего нет";
				}
			}

			var res = new OutputMessageText(message);

			return res;
		}


		public IMenuManager GetMenuManager()
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

			var dataMenu = new PageNavMenu();
			var dataBody = new DynamicMessage(GetUserData);
			var dataPage = new WidgetPage("other", "Мои данные", dataBody, dataMenu);

			dataMenu.AddAction(StartDataEntering);
			dataMenu.AddAction(StartAmeiEntering);

			mainMenu.PathTo(listPage);
			mainMenu.PathTo(dataPage);
			mainMenu.AddAction(StartSearching);

			mm.Define(mainPage);
			mm.Define(listPage);
			mm.Define(dataPage);

			return mm;
		}

		private DefaultCallback StartSearching => new("startCreation", "Создать заявку", Do_SearchAsync);
		private async Task Do_SearchAsync(SignedCallbackUpdate update)
		{
			if (update.Sender is IStatefulUser sender)
			{
				PassUser? user = context.PassUser.Where(user => user.TelegramId == sender.TelegramId).FirstOrDefault();

				if (user != null && user.IIN != null)
				{
					sender.State = InputFullNameState;
					await update.Owner.DeliveryService.ReplyToSender("Введите ФИО или \"Выйти\"", update);
				}
				else
				{
					await update.Owner.DeliveryService.ReplyToSender("Вы не авторизированы в нашей системе, пожалуйста, введите ваши данные", update);

					var mm = update.Owner.ResolveService<IMenuManager>();
					var page = mm.GetDefined("other");

					await mm.PushPageAsync(page, update);
				}
			}

		}

		private DefaultTextInput ExitInput => new("Выйти", Do_ExitInputCityAsync);
		private async Task Do_ExitInputCityAsync(SignedMessageTextUpdate update)
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

		private DefaultCallback StartDataEntering => new("startEnterData", "Ввести данные", Do_InputUserFullNameAsync);
		private async Task Do_InputUserFullNameAsync(SignedCallbackUpdate update)
		{
			if (update.Sender is IStatefulUser sender)
			{

				PassUser? user = context.PassUser.Where(user => user.TelegramId == sender.TelegramId).FirstOrDefault();

				if (user == null)
				{
					user = new PassUser();
					user.TelegramId = (int)sender.TelegramId;

					context.PassUser.Add(user);
					await context.SaveChangesAsync();
				}

				sender.State = InputUserFullNameState;
				await update.Owner.DeliveryService.ReplyToSender("1. Введите ваше ФИО или \"Выйти\"", update);
			}
		}

		private AnyInput InputUserFullname => new("userFullName", Do_UserFullnameInputAsync);

		private async Task Do_UserFullnameInputAsync(SignedMessageTextUpdate update)
		{
			if (update.Sender is IStatefulUser sender)
			{
				PassUser user = context.PassUser.Where(user => user.TelegramId == sender.TelegramId).First();

				user.Fullname = update.Text;

				context.PassUser.Update(user);

				await context.SaveChangesAsync();

				sender.State = InputIINState;

				var message = new OutputMessageText($"2. Введите ваш ИИН или \"Выйти\"");

				await update.Owner.DeliveryService.ReplyToSender(message, update);
			}
		}

		private NumberInput InputUserIIN => new("userIIN", Do_UserIINInputAsync);

		private async Task Do_UserIINInputAsync(SignedMessageTextUpdate update)
		{
			if (update.Sender is IStatefulUser sender)
			{
				PassUser user = context.PassUser.Where(user => user.TelegramId == sender.TelegramId).First();

				user.IIN = update.Text;

				context.PassUser.Update(user);

				await context.SaveChangesAsync();

				sender.State = InputTelephoneState;

				var message = new OutputMessageText($"3. Введите ваш Телефон или \"Выйти\"");

				await update.Owner.DeliveryService.ReplyToSender(message, update);
			}
		}

		private NumberInput InputUserTelephone => new("userTelephone", Do_UserTelephoneInputAsync);

		private async Task Do_UserTelephoneInputAsync(SignedMessageTextUpdate update)
		{
			if (update.Sender is IStatefulUser sender)
			{
				PassUser user = context.PassUser.Where(user => user.TelegramId == sender.TelegramId).First();

				user.Telephone = update.Text;

				context.PassUser.Update(user);

				await context.SaveChangesAsync();

				sender.State = DefaultState;

				var message = new OutputMessageText($"Вы заполнили все данные");

				await update.Owner.DeliveryService.ReplyToSender(message, update);

				await update.Owner.DeliveryService.ReplyToSender(message, update);

				var mm = update.Owner.ResolveService<IMenuManager>();

				var page = mm.GetDefined("other");

				await mm.PushPageAsync(page, update);
			}
		}
		
		private DefaultCallback StartAmeiEntering => new("startEnterAmei", "Ввести AMEI (для сотрудников AMT)", Do_InputUserAmeiAsync);
		private async Task Do_InputUserAmeiAsync(SignedCallbackUpdate update)
		{
			if (update.Sender is IStatefulUser sender)
			{
				sender.State = InputAmeiState;
				await update.Owner.DeliveryService.ReplyToSender("Введите ваш AMEI или \"Выйти\"", update);
			}
		}

		private AnyInput InputUserAmei => new("userAMEI", Do_UserAMEIInputAsync);
		private async Task Do_UserAMEIInputAsync(SignedMessageTextUpdate update)
		{
			if (update.Sender is IStatefulUser sender)
			{
				Amei? amei = context.Ameis.Where(u => u.Usrid == int.Parse(update.Message.Text)).FirstOrDefault();

				OutputMessage? message = null;

				if(amei != null)
				{
					PassUser user = context.PassUser.Where(user => user.TelegramId == sender.TelegramId).First();

					user.Amei = amei;

					user.Telephone = amei.Phone;
					user.Fullname = amei.Nachn + " " + amei.Vorna + " " + amei.Midnm;
					user.IIN = amei.Perid;

					sender.State = DefaultState;

					context.Update(user);
					await context.SaveChangesAsync();

					message = new OutputMessageText($"Вы заполнили все данные");

					await update.Owner.DeliveryService.ReplyToSender(message, update);

					var mm = update.Owner.ResolveService<IMenuManager>();

					var page = mm.GetDefined("other");

					await mm.PushPageAsync(page, update);
				}
				else
				{
					message = new OutputMessageText($"Пользователь с таким номером не найден! Попробуйте ещё раз.");
					await update.Owner.DeliveryService.ReplyToSender(message, update);
				}
			}
		}

		private AnyInput InputFullName => new("fullname", Do_InputFullNameAsync);
		private async Task Do_InputFullNameAsync(SignedMessageTextUpdate update)
		{
			string fullName = update.Text;
			var message = new OutputMessageText("Вы ввели: " + fullName);

			await update.Owner.DeliveryService.ReplyToSender(message, update);

			var users = context.Ameis.Where(value => EF.Functions.Like(value.Nachn + " " + value.Vorna + " " + value.Midnm, $"%{fullName}%")).ToList();

			if (users.Count > 0)
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

		private AnyInput InputReason => new("reason", Do_InputReasonAsync);
		private async Task Do_InputReasonAsync(SignedMessageTextUpdate update)
		{
			string reason = update.Text;
			var message = new OutputMessageText("Вы ввели: " + reason);

			await update.Owner.DeliveryService.ReplyToSender(message, update);

			var botUser = update.Sender as BotUser;
			botUser.State = SelectUserState;
			botUser.Reason = reason;

			await Do_ShowRequestMenu(update);
		}

		private int PageCount = 5;

		private DefaultCallback NextPage => new("следующая_страница", "Следующая страница", Do_NextPage);
		private DefaultCallback PrevPage => new("предыдущая_страница", "Предыдущая страница", Do_PrevPage);

		private async Task Do_PrevPage(SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser sender)
			{
				sender.UserPages.Page--;

				await Page(update);
			}
		}

		private async Task Do_NextPage(SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser sender)
			{
				sender.UserPages.Page++;

				await Page(update);
			}
		}

		private DefaultCallback CancelPage => new("отмена", "Отмена", Do_CancelPage);

		private async Task Do_CancelPage(SignedCallbackUpdate update)
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

		private async Task Page(SignedCallbackUpdate update)
		{
			BotUser sender = (BotUser)update.Sender;

			if (sender.UserPages.Users is null)
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

		private BotArgedCallback<IntWrapper> SelectUser => new(new LabeledData("Выбор", "SelectUser"), Do_SelectUserAsync);
		private async Task Do_SelectUserAsync(IntWrapper args, SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser user)
			{
				user.UserPages.Page = 0;

				var amei = context.Ameis.Where(User => User.Usrid == args.Value).First();

				user.SelectedUser = amei;

				var message = new OutputMessageText(update.Message.Text + $"\n\nВы выбрали {amei.Nachn + " " + amei.Vorna + " " + amei.Midnm}")
				{
					Menu = null,
				};
				await update.Owner.DeliveryService.ReplyToSender(new EditWrapper(message, update.TriggerMessageId), update);

				user.State = InputReasonState;
				await update.Owner.DeliveryService.ReplyToSender("Введите причину посещения или \"Выйти\"", update);
			}
		}
		private async Task Do_CreateRequest(SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser user)
			{
				var msg = "";

				try
				{
					await requestService.CreateRequest(user);

					msg = update.Message.Text + $"\n\nЗаявка успешно создана!";

				}
				catch (Exception ex)
				{
					msg = update.Message.Text + $"\n\nНе удалось создать заявку :(";
				}

				var message = new OutputMessageText(msg)
				{
					Menu = null
				};

				await update.Owner.DeliveryService.ReplyToSender(new EditWrapper(message, update.TriggerMessageId), update);
			}
		}
	}
}
