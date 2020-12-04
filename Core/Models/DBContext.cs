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

        private readonly ILoggerFactory _loggerFactory;
        public DBContext() { }

        public DBContext(DbContextOptions<DBContext> options, ILoggerFactory loggerFactory) : base(options)
        {
            _loggerFactory = loggerFactory;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Allow null if you are using an IDesignTimeDbContextFactory
            if (_loggerFactory != null)
            {
                if (Debugger.IsAttached)
                {
                    // Probably shouldn't log sql statements in production
                    // optionsBuilder.UseLoggerFactory(_loggerFactory);
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.HasPostgresExtension("pg_trgm");

            builder.Entity<Manifest<BNetLib.Models.Versions[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.Versions[]>>().HasKey(x => new { x.Code, x.Seqn });
            builder.Entity<Manifest<BNetLib.Models.Versions[]>>().ToTable("versions");

            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>().HasKey(x => new { x.Code, x.Seqn });
            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>().ToTable("bgdl");

            builder.Entity<Manifest<BNetLib.Models.CDN[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.CDN[]>>().HasKey(x => new { x.Code, x.Seqn });
            builder.Entity<Manifest<BNetLib.Models.CDN[]>>().ToTable("cdns");

            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().HasKey(x => x.Seqn);
            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().ToTable("summary");
        }
    }
}
