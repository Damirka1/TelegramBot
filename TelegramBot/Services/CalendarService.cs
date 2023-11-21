using SKitLs.Bots.Telegram.AdvancedMessages.Model.Menus;
using SKitLs.Bots.Telegram.ArgedInteractions.Argumentation;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using TelegramBot.Services.Calendar;
using System.Globalization;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Messages.Text;
using SKitLs.Bots.Telegram.AdvancedMessages.Model;
using SKitLs.Bots.Telegram.ArgedInteractions.Interactions.Model;
using SKitLs.Bots.Telegram.Core.Model.Interactions;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting.Signed;
using TelegramBot.Dtos;
using TelegramBot.Users;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.VisualBasic;
using Constants = TelegramBot.Services.Calendar.Constants;
using SKitLs.Bots.Telegram.AdvancedMessages.Model.Menus.Inline;
using SKitLs.Bots.Telegram.Core.Model.DeliverySystem.Prototype;

namespace TelegramBot.Services
{
	internal class CalendarService
	{
		private DateTimeFormatInfo Dtfi = new CultureInfo("ru-RU", false).DateTimeFormat;

		private Func<SignedCallbackUpdate, Task> Do_CreateRequest;

		public BotArgedCallback<StringWrapper> CalendarCallBack => new(new LabeledData("календарь", "calendar"), Do_CalendarAsync);
		private async Task Do_CalendarAsync(StringWrapper args, SignedCallbackUpdate update)
		{
			if (update.Sender is BotUser user)
			{
				if(args.Value != " ")
				{
					var sp = args.Value.Split('/');

					var date = DateTime.Parse(args.Value.Substring(sp[0].Length + 1));

					switch(sp[0] + '/')
					{
						case Constants.ChangeTo:
							await ShowCalendarByDate(update, date);
							break;
						case Constants.YearMonthPicker:
							await ShowMonthCalendar(update, date);
							break;
						case Constants.PickDate:
							await SelectTime(update, date);
							break;
						case Constants.PickTime:
							await SelectDuration(update, date);
							break;
						case Constants.PickDuration:
							await Finish(update, date);
							break;
						case Constants.PickFinish:
							await Do_CreateRequest(update);
							break;
					}
				}
			}
		}

		public void SetFinishFunc(Func<SignedCallbackUpdate, Task> Do_CreateRequest)
		{
			this.Do_CreateRequest = Do_CreateRequest;
		}

		private void AddYear(InlineMenu menu, DateTime date)
		{
			menu.Add($"» {date.ToString("Y", Dtfi)} «", CalendarCallBack, new StringWrapper($"{Constants.YearMonthPicker}{date.ToString(Constants.DateFormat)}"), true);
		}

		private void AddDayOfWeek(InlineMenu menu)
		{
			var firstDayOfWeek = (int)Dtfi.FirstDayOfWeek;

			menu.ColumnsCount = 7;

			for (int i = 0; i < 7; i++)
			{
				menu.Add(Dtfi.AbbreviatedDayNames[(firstDayOfWeek + i) % 7], CalendarCallBack, new StringWrapper(""), false);
			}
		}

		private void AddMonth(InlineMenu menu, DateTime date)
		{
			var firstDayOfMonth = new DateTime(date.Year, date.Month, 1);
			var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1).Day;

			for (int dayOfMonth = 1, weekNum = 0; dayOfMonth <= lastDayOfMonth; weekNum++)
			{
				AddNewWeek(weekNum, ref dayOfMonth);
			}

			void AddNewWeek(int weekNum, ref int dayOfMonth)
			{
				for (int dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
				{
					if ((weekNum == 0 && dayOfWeek < FirstDayOfWeek())
					   ||
					   dayOfMonth > lastDayOfMonth
					)
					{
						menu.Add(" ", CalendarCallBack, new StringWrapper(""), false);
						continue;
					}

					menu.Add(dayOfMonth.ToString(), CalendarCallBack, new StringWrapper($"{Constants.PickDate}{new DateTime(date.Year, date.Month, dayOfMonth).ToString(Constants.DateFormat)}"), false);

					dayOfMonth++;
				}

				int FirstDayOfWeek() =>
					(7 + (int)firstDayOfMonth.DayOfWeek - (int)Dtfi.FirstDayOfWeek) % 7;
			}
		}

		private void AddControls(InlineMenu menu, DateTime date)
		{
			menu.Add("<", CalendarCallBack, new StringWrapper($"{Constants.ChangeTo}{date.AddMonths(-1).ToString(Constants.DateFormat)}"), false);
			menu.Add(" ", CalendarCallBack, new StringWrapper(""), false);
			menu.Add(">", CalendarCallBack, new StringWrapper($"{Constants.ChangeTo}{date.AddMonths(+1).ToString(Constants.DateFormat)}"), false);
		}

		public async Task ShowCalendar(ISignedUpdate update)
		{
			var menu = new InlineMenu()
			{
				Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
			};

			var date = DateTime.Now;

			AddYear(menu, date);

			AddDayOfWeek(menu);

			AddMonth(menu, date);

			AddControls(menu, date);

			var message = new OutputMessageText("Выберите дату посещения")
			{
				Menu = menu,
			};

			await update.Owner.DeliveryService.AnswerSenderAsync(await message.BuildContentAsync(update), update);
		}

		private async Task ShowCalendarByDate(SignedCallbackUpdate update, DateTime date)
		{
			var menu = new InlineMenu()
			{
				Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
			};

			AddYear(menu, date);

			AddDayOfWeek(menu);

			AddMonth(menu, date);

			AddControls(menu, date);

			var message = new OutputMessageText("Выберите дату посещения")
			{
				Menu = menu,
			};

			await update.Owner.DeliveryService.AnswerSenderAsync(new EditWrapper(await message.BuildContentAsync(update), update.TriggerMessageId), update);
		}

		private async Task ShowMonthCalendar(SignedCallbackUpdate update, DateTime date)
		{
			var menu = new InlineMenu()
			{
				Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
			};

			menu.Add($"» {date.ToString("yyyy", Dtfi)} «", CalendarCallBack, new StringWrapper($"{Constants.YearMonthPicker}{date.ToString(Constants.DateFormat)}"), true);

			menu.ColumnsCount = 4;

			for (int i = 1; i < 13; i++)
			{
				menu.Add($"{Dtfi.GetMonthName(i)}", CalendarCallBack, new StringWrapper($"{Constants.ChangeTo}{new DateTime(date.Year, i, 1).ToString(Constants.DateFormat)}"), false);
			}

			menu.Add("<", CalendarCallBack, new StringWrapper($"{Constants.YearMonthPicker}{date.AddYears(-1).ToString(Constants.DateFormat)}"), false);
			menu.Add(" ", CalendarCallBack, new StringWrapper(""), false);
			menu.Add(">", CalendarCallBack, new StringWrapper($"{Constants.YearMonthPicker}{date.AddYears(+1).ToString(Constants.DateFormat)}"), false);

			var message = new OutputMessageText("Выберите дату посещения")
			{
				Menu = menu,
			};

			await update.Owner.DeliveryService.AnswerSenderAsync(new EditWrapper(await message.BuildContentAsync(update), update.TriggerMessageId), update);
		}

		private async Task SelectTime(SignedCallbackUpdate update, DateTime date)
		{
			var menu = new InlineMenu()
			{
				Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
			};

			menu.Add($"» {date.ToString("D", Dtfi)} «", CalendarCallBack, new StringWrapper(""), true);

			menu.ColumnsCount = 4;

			for(int i = 0; i < 8; i++)
			{
				int startHour = 8 + i;
				var time = new DateTime(date.Year, date.Month, date.Day, startHour, 0, 0);
				menu.Add($"{time.ToString("t" ,Dtfi)}", CalendarCallBack, new StringWrapper($"{Constants.PickTime}{time.ToString(Constants.DateTimeFormat)}"), false);
				
				var timeHalf = time.AddMinutes(30);
				menu.Add($"{timeHalf.ToString("t", Dtfi)}", CalendarCallBack, new StringWrapper($"{Constants.PickTime}{timeHalf.ToString(Constants.DateTimeFormat)}"), false);
			}

			menu.Add("-", CalendarCallBack, new StringWrapper(""), false);
			menu.Add("Назад", CalendarCallBack, new StringWrapper($"{Constants.ChangeTo}{date.ToString(Constants.DateFormat)}"), false);
			menu.Add("-", CalendarCallBack, new StringWrapper(""), false);


			var message = new OutputMessageText("Выберите дату посещения" + ": " +
				$"{date.ToString("D", Dtfi)}\n\n" +
				$"Выберите время")
			{
				Menu = menu,
			};

			await update.Owner.DeliveryService.AnswerSenderAsync(new EditWrapper(await message.BuildContentAsync(update), update.TriggerMessageId), update);
		}

		private async Task SelectDuration(SignedCallbackUpdate update, DateTime date)
		{
			var botUser = update.Sender as BotUser;

			botUser.ScheduleStart = date;

			var menu = new InlineMenu()
			{
				Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
			};

			menu.Add($"» {date.ToString("g", Dtfi)} «", CalendarCallBack, new StringWrapper(""), true);

			menu.ColumnsCount = 4;

			menu.Add($"30 минут", CalendarCallBack, new StringWrapper($"{Constants.PickDuration}{date.AddMinutes(30).ToString(Constants.DateTimeFormat)}"), false);
			menu.Add($"1 час", CalendarCallBack, new StringWrapper($"{Constants.PickDuration}{date.AddHours(1).ToString(Constants.DateTimeFormat)}"), false);
			menu.Add($"2 часа", CalendarCallBack, new StringWrapper($"{Constants.PickDuration}{date.AddHours(2).ToString(Constants.DateTimeFormat)}"), false);
			menu.Add($"3 часа", CalendarCallBack, new StringWrapper($"{Constants.PickDuration}{date.AddHours(3).ToString(Constants.DateTimeFormat)}"), false);

			menu.Add($"До конца рабочего дня", CalendarCallBack, new StringWrapper($"{Constants.PickDuration}{new DateTime(date.Year, date.Month, date.Day, 16, 0, 0).ToString(Constants.DateTimeFormat)}"), true);

			menu.Add("-", CalendarCallBack, new StringWrapper(""), false);
			menu.Add("Назад", CalendarCallBack, new StringWrapper($"{Constants.PickDate}{date.ToString(Constants.DateFormat)}"), false);
			menu.Add("-", CalendarCallBack, new StringWrapper(""), false);


			var message = new OutputMessageText(update.Message.Text + ": " +
				$"{date.ToString("t", Dtfi)}\n\n" +
				$"Выберите длительность посещения")
			{
				Menu = menu,
			};

			await update.Owner.DeliveryService.AnswerSenderAsync(new EditWrapper(await message.BuildContentAsync(update), update.TriggerMessageId), update);
		}

		private async Task Finish(SignedCallbackUpdate update, DateTime date)
		{
			var botUser = update.Sender as BotUser;

			botUser.ScheduleEnd = date;

			var menu = new InlineMenu()
			{
				Serializer = update.Owner.ResolveService<IArgsSerializeService>(),
				ColumnsCount = 4
			};

			menu.Add($"» {date.ToString("D", Dtfi)}{", "}{botUser.ScheduleStart.ToString("t", Dtfi)}{" - "}{botUser.ScheduleEnd.ToString("t", Dtfi)} «", CalendarCallBack, new StringWrapper(""), true);
			
			menu.Add("Подтвердить", CalendarCallBack, new StringWrapper($"{Constants.PickFinish}{date.ToString(Constants.DateFormat)}"), true);

			menu.Add("-", CalendarCallBack, new StringWrapper(""), false);
			menu.Add("Назад", CalendarCallBack, new StringWrapper($"{Constants.PickTime}{botUser.ScheduleStart.ToString(Constants.DateTimeFormat)}"), false);
			menu.Add("-", CalendarCallBack, new StringWrapper(""), false);

			var duration = (botUser.ScheduleEnd - botUser.ScheduleStart);

			var text = "";

			if(duration.Hours < 1 )
			{
				text = "30 минут";
			} 
			else if (duration.Hours == 1)
			{
				text = $"{duration.Hours} час";
			}
			else if (duration.Hours < 5)
			{
				text = $"{duration.Hours} часа";
			}
			else
			{
				text = $"{duration.Hours} часов";
			}

			var message = new OutputMessageText(update.Message.Text + ": " +
				$"{text}")
			{
				Menu = menu,
			};

			await update.Owner.DeliveryService.AnswerSenderAsync(new EditWrapper(await message.BuildContentAsync(update), update.TriggerMessageId), update);
		}
	}
}
