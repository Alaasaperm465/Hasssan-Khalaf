using Hassann_Khala.Domain;
using Microsoft.EntityFrameworkCore;

namespace InfraStructure.Context
{
    public class DBContext : DbContext
    {
        public DBContext(DbContextOptions<DBContext> options) : base(options)
        {
        }

        // Parameterless constructor left out intentionally to encourage DI usage.

        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<Section> Sections { get; set; } = null!;
        public DbSet<Stock> Stocks { get; set; } = null!;
        public DbSet<Inbound> Inbounds { get; set; } = null!;
        public DbSet<InboundDetail> InboundDetails { get; set; } = null!;
        public DbSet<Outbound> Outbounds { get; set; } = null!;
        public DbSet<OutboundDetail> OutboundDetails { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<ProductStock> ProductStocks { get; set; } = null!;
        public DbSet<SectionStock> SectionStocks { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // If options are not configured (e.g. when EF tools create the context without running the host),
            // try to configure SQL Server from the environment variable fallback. This prevents "no database provider"
            // errors when IMigrator is resolved.
            if (!optionsBuilder.IsConfigured)
            {
                var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
                if (!string.IsNullOrWhiteSpace(envConn))
                {
                    optionsBuilder.UseSqlServer(envConn);
                }
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>().HasIndex(c => c.Name).IsUnique();

            modelBuilder.Entity<Product>().HasQueryFilter(p => p.IsActive);

            modelBuilder.Entity<Stock>().HasIndex(s => new { s.ClientId, s.ProductId, s.SectionId }).IsUnique();

            modelBuilder.Entity<ProductStock>().HasIndex(p => new { p.ClientId, p.ProductId }).IsUnique();
            modelBuilder.Entity<SectionStock>().HasIndex(s => new { s.ClientId, s.SectionId }).IsUnique();

            // Configure relationships from Stock side to avoid requiring navigation collections on Client/Product/Section
            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Client)
                .WithMany()
                .HasForeignKey(s => s.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Product)
                .WithMany(p => p.Stocks)
                .HasForeignKey(s => s.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Stock>()
                .HasOne(s => s.Section)
                .WithMany()
                .HasForeignKey(s => s.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductStock>()
                .HasOne(ps => ps.Product)
                .WithMany()
                .HasForeignKey(ps => ps.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ProductStock>()
                .HasOne(ps => ps.Client)
                .WithMany()
                .HasForeignKey(ps => ps.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SectionStock>()
                .HasOne(ss => ss.Section)
                .WithMany()
                .HasForeignKey(ss => ss.SectionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SectionStock>()
                .HasOne(ss => ss.Client)
                .WithMany()
                .HasForeignKey(ss => ss.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // relationships for inbound/outbound details
            modelBuilder.Entity<InboundDetail>().HasOne(d => d.Inbound).WithMany(i => i.Details).HasForeignKey(d => d.InboundId).OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<OutboundDetail>().HasOne(d => d.Outbound).WithMany(i => i.Details).HasForeignKey(d => d.OutboundId).OnDelete(DeleteBehavior.Cascade);

            // Seed sections
            var sections = new[]
            {
                new Section { Id = 1, Name = "S1" },
                new Section { Id = 2, Name = "S2" },
                new Section { Id = 3, Name = "S3" },
                new Section { Id = 4, Name = "S4" },
                new Section { Id = 5, Name = "S5" },
                new Section { Id = 6, Name = "S6" },
                new Section { Id = 7, Name = "S7" },
                new Section { Id = 8, Name = "S8" },
            };
            modelBuilder.Entity<Section>().HasData(sections);

            // Seed users with integer ids and plain text passwords (testing only)
            modelBuilder.Entity<User>().HasData(
                new User { Id = 1, Username = "admin", Password = "123456", Role = Roles.Admin, CreatedAt = new DateTime(2026, 2, 19) },
                new User { Id = 2, Username = "storekeeper", Password = "123456", Role = Roles.StoreKeeper, CreatedAt = new DateTime(2026, 2, 19) }
            );
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
