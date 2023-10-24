using SKitLs.Bots.Telegram.DataBases.Prototype;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Entities
{
	internal class PassUserBot : IBotDisplayable, IOwnedData
	{
		public string Fullname { get; set; } = "";
		public string IIN { get; set; } = "";
		public string Telephone { get; set; } = "";
		public int TelegramId { get; set; }

		public long BotArgId { get; set; }
		public void UpdateId(long id) => BotArgId = id;
		public bool IsOwnedBy(long userId) => TelegramId == userId;

		public string ListDisplay() => Fullname;
		public string ListLabel() => Fullname;
		public string FullDisplay(params string[] args) => GetDisplay();

		public string GetDisplay() => $"ФИО: {Fullname}\n\nИИН: {IIN}\n\nТелефон: {Telephone}";
	}
}
