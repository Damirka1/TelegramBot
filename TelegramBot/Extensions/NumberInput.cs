using SKitLs.Bots.Telegram.Core.Model.Interactions.Defaults;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting.Signed;
using SKitLs.Bots.Telegram.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Extensions
{
	internal class NumberInput : DefaultBotAction<SignedMessageTextUpdate>
	{
		public NumberInput(string anyId, BotInteraction<SignedMessageTextUpdate> action) : base("systemAny." + anyId, action)
		{ }

		// Действие должно реагировать на любой ввод, поэтому возвращаем true без проверок.
		public override bool ShouldBeExecutedOn(SignedMessageTextUpdate update)
		{
			bool result = int.TryParse(update.Text, out _);

			return result;
		}
	}
}
