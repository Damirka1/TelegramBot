using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Entities
{
	public class PassStatus
	{
		public enum StatusEnum
		{
			Created,
			InProgress,
			Declined,
			Accepted
		}

		public int Id { get; set; }
		public DateTime Created { get; set; }
		public StatusEnum Status { get; set; }
	}
}
