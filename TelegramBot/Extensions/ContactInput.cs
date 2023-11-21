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
	internal class ContactInput : DefaultBotAction<SignedCallbackUpdate>
	{
		public ContactInput(string anyId, BotInteraction<SignedCallbackUpdate> action) : base("systemAny." + anyId, action)
		{ }

		public override bool ShouldBeExecutedOn(SignedCallbackUpdate update) => update.Message.Contact != null || update.Message.Text.Length > 0;
	}
}
