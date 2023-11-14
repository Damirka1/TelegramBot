using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Entities
{
	[Table("PASS_REQUEST")]
	public class PassRequest 
	{
		public int Id { get; set; }
		public PassUser From { get; set; }
		public Amei To { get; set; }
		public PassSchedule PassSchedule { get; set; }
		public List<PassStatus> PassStatus { get; set; }
		public DateTime Created { get; set; }
		public String Reason { get; set; }
	}
}
