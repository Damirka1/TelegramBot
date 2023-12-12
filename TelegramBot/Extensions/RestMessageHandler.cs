using SKitLs.Bots.Telegram.Core.Model;
using SKitLs.Bots.Telegram.Core.Model.Interactions;
using SKitLs.Bots.Telegram.Core.Model.UpdateHandlers;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting;
using SKitLs.Bots.Telegram.Core.Model.UpdatesCasting.Signed;
using SKitLs.Bots.Telegram.Core.Prototype;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TelegramBot.Services;

namespace TelegramBot.Extensions
{
	internal class RestMessageHandler : IUpdateHandlerBase<SignedMessageUpdate>
	{
		private BotManager owner;
		public BotManager Owner { get => owner; set => owner = value; }

		public Action<object, BotManager>? OnCompilation => null;

		private BotService BotService;

		public RestMessageHandler(BotService botService)
		{
			BotService = botService;
		}

		public SignedMessageUpdate CastUpdate(ICastedUpdate update, IBotUser? sender)
		{
			throw new NotImplementedException();
		}

		public List<IBotAction> GetActionsContent()
		{
			throw new NotImplementedException();
		}

		public Task HandleUpdateAsync(SignedMessageUpdate update)
		{
			var contact = update.Message.Contact;

			if (contact != null)
			{
				var message = "Ваш номер: " + contact.PhoneNumber;

				update.Owner.Bot.SendTextMessageAsync(chatId: update.ChatId, text: message, replyMarkup: new ReplyKeyboardRemove());

				return BotService.AuthorizeUser(contact, (Users.BotUser) update.Sender, update);
			}

			return Task.CompletedTask;
		}

		public Task HandleUpdateAsync(ICastedUpdate update, IBotUser? sender)
		{
			throw new NotImplementedException();
		}
	}
}
