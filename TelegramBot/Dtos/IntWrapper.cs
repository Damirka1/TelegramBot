using SKitLs.Bots.Telegram.ArgedInteractions.Argumentation.Prototype;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Dtos
{
	// Где IntWrapper - это простая class обёртка для struct int
	internal class IntWrapper
	{
		[BotActionArgument(0)]
		public int Value { get; set; }

		public IntWrapper() { }
		public IntWrapper(int value) => Value = value;
	}
}
