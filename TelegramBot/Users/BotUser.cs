using SKitLs.Bots.Telegram.Stateful.Model;
using SKitLs.Bots.Telegram.Stateful.Prototype;
using TelegramBot.Dtos;
using TelegramBot.Entities;

namespace TelegramBot.Users
{
    internal class BotUser : IStatefulUser
    {
        public IUserState State { get; set; }
        public long TelegramId { get; set; }
		public UserList UserPages { get; set; }

		public Amei? SelectedUser { get; set; }

        public DateTime ScheduleStart { get; set; }

		public DateTime ScheduleEnd { get; set; }

        public String Reason { get; set; }

		public BotUser(long telegramId)
        {
            State = new DefaultUserState();
            TelegramId = telegramId;
            UserPages = new UserList();
            SelectedUser = null;
        }

        public void ResetState() => State = new DefaultUserState();
    }
}