using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot.Entities
{
    internal class BankInfo
    {
        public string Disclaimer { get; set; }
        public DateTime Date { get; set; }
        public int Timestamp { get; set; }
        public string Base { get; set; }

        public Dictionary<string, double> Rates { get; set; }

    }
}
