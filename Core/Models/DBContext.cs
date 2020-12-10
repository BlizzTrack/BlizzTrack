using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Core.Models
{
    public class DBContext : DbContext
    {
        public DbSet<Manifest<BNetLib.Models.Versions[]>> Versions { get; set; }
        public DbSet<Manifest<BNetLib.Models.BGDL[]>> BGDL { get; set; }
        public DbSet<Manifest<BNetLib.Models.CDN[]>> CDN { get; set; }
        public DbSet<Manifest<BNetLib.Models.Summary[]>> Summary { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("pg_trgm");

            builder.Entity<Manifest<BNetLib.Models.Versions[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.Versions[]>>().HasNoKey();
            builder.Entity<Manifest<BNetLib.Models.Versions[]>>().ToTable("versions");

            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>().HasNoKey();
            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>().ToTable("bgdl");

            builder.Entity<Manifest<BNetLib.Models.CDN[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.CDN[]>>().HasNoKey();
            builder.Entity<Manifest<BNetLib.Models.CDN[]>>().ToTable("cdns");

            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().HasNoKey();
            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().ToTable("summary");
        }
    }
}
