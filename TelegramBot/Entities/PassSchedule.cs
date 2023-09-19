using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Entities
{
	public class PassSchedule
	{
		public int Id { get; set; }
		public DateTime Day { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}
}
