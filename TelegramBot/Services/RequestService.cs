using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Entities;
using TelegramBot.Users;

namespace TelegramBot.Services
{
	internal class RequestService
	{
		private OracleContext context;
		public RequestService(OracleContext context) 
		{
			this.context = context;
		}

		public async Task CreateRequest(BotUser botUser, DateTime time)
		{
			if (botUser.SelectedUser == null)
				throw new ArgumentNullException(nameof(botUser.SelectedUser));

			PassUser passUser = context.PassUser.Where(user => user.TelegramId == botUser.TelegramId).First();

			var now = DateTime.Now;

			PassRequest passRequest = new PassRequest();

			passRequest.Created = now;

			passRequest.From = passUser;

			passRequest.To = botUser.SelectedUser;

			var passStatus = new PassStatus();

			passStatus.Created = now;

			passRequest.PassStatus = new List<PassStatus> { passStatus };

			var passSchedule = new PassSchedule();
			passSchedule.Day = time.Date;
			passSchedule.Start = time;
			passSchedule.End = time.AddHours(1);

			passRequest.PassSchedule = passSchedule;

			context.PassRequest.Add(passRequest);
			context.PassStatus.Add(passStatus);
			context.PassSchedule.Add(passSchedule);

			await context.SaveChangesAsync();
		}
	}
}
