using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Core.Models
{
    public class DBContext : IdentityDbContext<User>, IDataProtectionKeyContext
    {
        public DbSet<Manifest<BNetLib.Models.Versions[]>> Versions { get; set; }
        public DbSet<Manifest<BNetLib.Models.BGDL[]>> BGDL { get; set; }
        public DbSet<Manifest<BNetLib.Models.CDN[]>> CDN { get; set; }
        public DbSet<Manifest<BNetLib.Models.Summary[]>> Summary { get; set; }
        public DbSet<GameConfig> GameConfigs { get; set; }
        public DbSet<GameParents> GameParents { get; set; }
        public DbSet<GameChildren> GameChildren { get; set; }
        public DbSet<GameCompany> GameCompanies { get; set; }
        public DbSet<PatchNote> PatchNotes { get; set; }
        public DbSet<Catalog> Catalogs { get; set; }
        public DbSet<Assets> Assets { get; set; }
        
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("pg_trgm");

            builder.Entity<GameConfig>().ToTable("game_configs");

            builder.Entity<Manifest<BNetLib.Models.Versions[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.Versions[]>>().HasKey(x => new {x.Code, x.Seqn});
            builder.Entity<Manifest<BNetLib.Models.Versions[]>>()
                .HasIndex(x => x.ConfigId)
                .IsUnique(false);
            builder.Entity<Manifest<BNetLib.Models.Versions[]>>()
                .HasOne(x => x.Config)
                .WithOne()
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Manifest<BNetLib.Models.Versions[]>>().ToTable("versions");

            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>().HasKey(x => new { x.Code, x.Seqn });
            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>()
                .HasIndex(x => x.ConfigId)
                .IsUnique(false);
            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>()
                .HasOne(x => x.Config)
                .WithOne()
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Manifest<BNetLib.Models.BGDL[]>>().ToTable("bgdl");

            builder.Entity<Manifest<BNetLib.Models.CDN[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.CDN[]>>().HasKey(x => new { x.Code, x.Seqn });
            builder.Entity<Manifest<BNetLib.Models.CDN[]>>()
                .HasIndex(x => x.ConfigId)
                .IsUnique(false);
            builder.Entity<Manifest<BNetLib.Models.CDN[]>>()
                .HasOne(x => x.Config)
                .WithOne()
                .OnDelete(DeleteBehavior.SetNull);
            builder.Entity<Manifest<BNetLib.Models.CDN[]>>().ToTable("cdns");

            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().Property(x => x.Indexed).HasDefaultValueSql("CURRENT_TIMESTAMP");
            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().HasKey(x => x.Seqn);
            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().Ignore(x => x.Parent);
            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().Ignore(x => x.Config);
            builder.Entity<Manifest<BNetLib.Models.Summary[]>>().ToTable("summary");

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