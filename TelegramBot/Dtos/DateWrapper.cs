using SKitLs.Bots.Telegram.ArgedInteractions.Argumentation.Prototype;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Dtos
{
	// Где DateWrapper - это простая class обёртка для class DateTime
	internal class DateWrapper
	{
		[BotActionArgument(0)]
		public string Value { get; set; }

		public DateWrapper() { }
		public DateWrapper(string value) => Value = value;
	}
}
