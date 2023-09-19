﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Entities
{
	public class PassRequest
	{
		public int Id { get; set; }
		public PassUser From { get; set; }
		public Amei To { get; set; }
		public PassSchedule PassSchedule { get; set; }
		public PassStatus PassStatus { get; set; }
	}
}
