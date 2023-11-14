using SKitLs.Bots.Telegram.ArgedInteractions.Argumentation.Prototype;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Dtos
{
	internal class StringWrapper
	{
		[BotActionArgument(0)]
		public string Value { get; set; }

		public StringWrapper() { }
		public StringWrapper(string value) => Value = value;
	}
}
