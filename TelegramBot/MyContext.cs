using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace TelegramBot
{
    internal class MyContext : DbContext
    {
        private string ConnectionString;
        public MyContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder _builder)
        {
            _builder.UseNpgsql(ConnectionString);
        }
    }
}
