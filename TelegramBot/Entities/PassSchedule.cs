using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Entities
{
	[Table("PASS_SCHEDULE")]
	public class PassSchedule
	{
		public int Id { get; set; }
		public DateTime Day { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
	}
}
