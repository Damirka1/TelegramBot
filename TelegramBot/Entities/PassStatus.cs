using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Entities
{
	[Table("PASS_STATUS")]
	public class PassStatus
	{
		public enum StatusEnum
		{
			Created,
			Accepted,
			Declined,
			Closed
		}

		public int Id { get; set; }
		public DateTime Created { get; set; }
		public StatusEnum Status { get; set; }
	}
}
