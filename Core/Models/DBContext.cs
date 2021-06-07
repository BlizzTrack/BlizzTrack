using BNetLib.Ribbit.Models;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core.Models
{
    public class DBContext : IdentityDbContext<User>, IDataProtectionKeyContext
    {
        public DbSet<Manifest<Versions[]>> Versions { get; set; }
        public DbSet<Manifest<BGDL[]>> BGDL { get; set; }
        public DbSet<Manifest<CDN[]>> CDN { get; set; }
        public DbSet<Manifest<Summary[]>> Summary { get; set; }
        public DbSet<GameConfig> GameConfigs { get; set; }
        public DbSet<GameParents> GameParents { get; set; }
        public DbSet<GameChildren> GameChildren { get; set; }
        public DbSet<GameCompany> GameCompanies { get; set; }
        public DbSet<PatchNote> PatchNotes { get; set; }
        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<Assets> Assets { get; set; }
        
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
        public DbSet<NotificationHistory> NotificationHistories { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("pg_trgm");

            builder.Entity<GameConfig>().ToTable("game_configs");

            builder.Entity<Manifest<Versions[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<Versions[]>>().HasKey(x => new {x.Code, x.Seqn});
            builder.Entity<Manifest<Versions[]>>()
                .HasIndex(x => x.ConfigId)
                .IsUnique(false);
            builder.Entity<Manifest<Versions[]>>()
                .HasOne(x => x.Config)
                .WithOne()
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Manifest<Versions[]>>().ToTable("versions");

            builder.Entity<Manifest<BGDL[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BGDL[]>>().HasKey(x => new { x.Code, x.Seqn });
            builder.Entity<Manifest<BGDL[]>>()
                .HasIndex(x => x.ConfigId)
                .IsUnique(false);
            builder.Entity<Manifest<BGDL[]>>()
                .HasOne(x => x.Config)
                .WithOne()
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Manifest<BGDL[]>>().ToTable("bgdl");

            builder.Entity<Manifest<CDN[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<CDN[]>>().HasKey(x => new { x.Code, x.Seqn });
            builder.Entity<Manifest<CDN[]>>()
                .HasIndex(x => x.ConfigId)
                .IsUnique(false);
            builder.Entity<Manifest<CDN[]>>()
                .HasOne(x => x.Config)
                .WithOne()
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Manifest<CDN[]>>().ToTable("cdns");

            builder.Entity<Manifest<Summary[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<Summary[]>>().HasKey(x => x.Seqn);
            builder.Entity<Manifest<Summary[]>>().Ignore(x => x.Parent);
            builder.Entity<Manifest<Summary[]>>().Ignore(x => x.Config);
            builder.Entity<Manifest<Summary[]>>().ToTable("summary");

            builder.Entity<Catalog>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Entity<GameChildren>()
                .HasOne(x => x.GameConfig)
                .WithOne(x => x.Owner)
                .HasPrincipalKey<GameChildren>(x => x.Code)
                .HasForeignKey<GameConfig>(x => x.Code);
            
            builder.Entity<GameParents>().Property(x => x.PatchNoteTool).HasDefaultValue("legacy");
            builder.Entity<GameParents>().Property(x => x.Visible).HasDefaultValue(true);
            builder.Entity<GameParents>().Property("OwnerId").HasDefaultValue(3);
            builder.Entity<GameParents>()
                .HasMany(c => c.Children)
                .WithOne(x => x.Parent)
                .HasPrincipalKey(x => x.Code)
                .HasForeignKey(x => x.ParentCode);

            builder.Entity<GameCompany>()
                .HasMany(x => x.Parents)
                .WithOne(x => x.Owner);
        }
    }
}