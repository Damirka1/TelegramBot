using SKitLs.Bots.Telegram.Stateful.Model;
using SKitLs.Bots.Telegram.Stateful.Prototype;
using TelegramBot.Dtos;

namespace TelegramBot.Users
{
    internal class BotUser : IStatefulUser
    {
        public IUserState State { get; set; }
        public long TelegramId { get; set; }
		public UserList UserPages { get; set; }

		public BotUser(long telegramId)
        {
            State = new DefaultUserState();
            TelegramId = telegramId;
            UserPages = new UserList();
        }

        public void ResetState() => State = new DefaultUserState();
    }
}