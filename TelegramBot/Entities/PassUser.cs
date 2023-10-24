using SKitLs.Bots.Telegram.DataBases.Prototype;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Entities
{
	public class PassUser
	{
		public int Id { get; set; }
		public string? Fullname { get; set; }
		public string? IIN { get; set; }
		public string? Telephone { get; set; }
		public int TelegramId { get; set; }
		public Amei? Amei { get; set;}
	}
}
