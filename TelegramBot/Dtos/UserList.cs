using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelegramBot.Entities;

namespace TelegramBot.Dtos
{
	internal class UserList
	{
		public int Page = 0;
		public List<Amei> Users { get; set; }

	}
}
