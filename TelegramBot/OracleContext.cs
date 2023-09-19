using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TelegramBot.Entities;

namespace TelegramBot
{
    internal partial class OracleContext : DbContext
    {
        private string ConnectionString;

		public OracleContext()
		{
			ConnectionString = "DATA SOURCE=10.21.198.23:1521/PNP1;PERSIST SECURITY INFO=True;USER ID=CHBTDEV; PASSWORD=xyz-0000; Pooling=True;";
		}
        public OracleContext(string connectionString)
        {
            ConnectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder _builder)
        {
            _builder.UseOracle(ConnectionString);
        }

		public DbSet<Amei> Ameis { get; set; }

		public DbSet<PassUser> PassUser { get; set; }

		public DbSet<PassRequest> PassRequest { get; set; }

		public DbSet<PassStatus> PassStatus { get; set; }

		public DbSet<PassSchedule> PassSchedule { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.HasDefaultSchema("CHBTDEV");

			modelBuilder.Entity<Amei>(entity =>
			{
				entity.HasKey(e => e.Usrid).HasName("AMEI_PK");

				entity.ToTable("AMEI");

				entity.HasIndex(e => new { e.Pernr, e.PernrOld }, "PERNR_IDX").IsUnique();

				entity.Property(e => e.Usrid)
					.HasPrecision(8)
					.ValueGeneratedNever()
					.HasColumnName("USRID");
				entity.Property(e => e.AmeiChief)
					.HasPrecision(8)
					.HasColumnName("AMEI_CHIEF");
				entity.Property(e => e.Bukrs)
					.HasMaxLength(4)
					.IsUnicode(false)
					.HasColumnName("BUKRS");
				entity.Property(e => e.Email)
					.HasMaxLength(100)
					.IsUnicode(false)
					.HasColumnName("EMAIL");
				entity.Property(e => e.Midnm)
					.HasMaxLength(50)
					.IsUnicode(false)
					.HasColumnName("MIDNM");
				entity.Property(e => e.Nachn)
					.HasMaxLength(50)
					.IsUnicode(false)
					.HasColumnName("NACHN");
				entity.Property(e => e.Perid)
					.HasMaxLength(20)
					.IsUnicode(false)
					.HasColumnName("PERID");
				entity.Property(e => e.Pernr)
					.HasPrecision(8)
					.HasColumnName("PERNR");
				entity.Property(e => e.PernrOld)
					.HasPrecision(8)
					.HasColumnName("PERNR_OLD");
				entity.Property(e => e.Persg)
					.HasMaxLength(1)
					.IsUnicode(false)
					.HasColumnName("PERSG");
				entity.Property(e => e.Phone)
					.HasMaxLength(100)
					.IsUnicode(false)
					.HasColumnName("PHONE");
				entity.Property(e => e.Plans)
					.HasPrecision(8)
					.HasColumnName("PLANS");
				entity.Property(e => e.Position)
					.HasMaxLength(40)
					.HasColumnName("POSITION");
				entity.Property(e => e.Rgubr)
					.HasPrecision(2)
					.HasColumnName("RGUBR");
				entity.Property(e => e.Robuv)
					.HasPrecision(2)
					.HasColumnName("ROBUV");
				entity.Property(e => e.Rodjd)
					.HasPrecision(2)
					.HasColumnName("RODJD");
				entity.Property(e => e.Rrost)
					.HasPrecision(3)
					.HasColumnName("RROST");
				entity.Property(e => e.Vdsk1)
					.HasMaxLength(14)
					.IsUnicode(false)
					.HasColumnName("VDSK1");
				entity.Property(e => e.Vorna)
					.HasMaxLength(50)
					.IsUnicode(false)
					.HasColumnName("VORNA");
				entity.Property(e => e.Zznachn)
					.HasMaxLength(80)
					.IsUnicode(false)
					.HasColumnName("ZZNACHN");
				entity.Property(e => e.Zzvorna)
					.HasMaxLength(80)
					.IsUnicode(false)
					.HasColumnName("ZZVORNA");
			});
			modelBuilder.HasSequence("CANDN");
			modelBuilder.HasSequence("LIST_FORM_HS_SEQ");
			modelBuilder.HasSequence("NOTIF_SEC");
			modelBuilder.HasSequence("SUGGBOOK");

			OnModelCreatingPartial(modelBuilder);
		}

		partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
	}
}
