using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TelegramBot.Entities;

namespace TelegramBot
{
    internal class ConvertService
    {
        private HttpClient client = new HttpClient();

        public BankInfo CurrentInfo { get; set; }

        public async Task<double> GetRubToKzt()
        {
            if(CurrentInfo == null || CurrentInfo.Date < DateTime.Now.Date)
            {
                var json = await client.GetStringAsync("https://www.cbr-xml-daily.ru/latest.js");
                var options = new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var info = JsonSerializer.Deserialize<BankInfo>(json, options);
                CurrentInfo = info;
            }

            return CurrentInfo.Rates["KZT"];
        }

    }
}
