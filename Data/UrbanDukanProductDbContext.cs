using Microsoft.EntityFrameworkCore;
using urban_dukan_product_service.Models;

namespace urban_dukan_product_service.Data
{
    public class UrbanDukanProductDbContext : DbContext
    {
        public UrbanDukanProductDbContext(DbContextOptions<UrbanDukanProductDbContext> options)
            : base(options)
        {
        }

        // Existing tables
        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();

        // New tables for Orders
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Product table config
            modelBuilder.Entity<Product>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Id).ValueGeneratedNever(); // incoming IDs from external provider

                b.Property(p => p.Title).HasMaxLength(250).IsRequired();
                b.Property(p => p.Description).HasMaxLength(2000);
                b.Property(p => p.Brand).HasMaxLength(250).IsRequired(false);
                b.Property(p => p.Price).HasColumnType("decimal(18,2)");
                b.Property(p => p.DiscountPercentage).HasColumnType("decimal(5,2)");
                b.Property(p => p.Rating).HasColumnType("decimal(3,2)");

                b.HasMany(p => p.Images)
                 .WithOne(i => i.Product!)
                 .HasForeignKey(i => i.ProductId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ProductImage table config
            modelBuilder.Entity<ProductImage>(b =>
            {
                b.HasKey(i => i.Id);
                b.Property(i => i.Url).HasMaxLength(2000).IsRequired();
            });

            // Order table config
            modelBuilder.Entity<Order>(b =>
            {
                b.HasKey(o => o.Id);
                b.Property(o => o.OrderDate).IsRequired();
                b.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)").IsRequired();
                b.Property(o => o.Status).HasMaxLength(50).IsRequired();

                b.HasMany(o => o.OrderItems)
                 .WithOne(oi => oi.Order!)
                 .HasForeignKey(oi => oi.OrderId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem table config
            modelBuilder.Entity<OrderItem>(b =>
            {
                b.HasKey(oi => oi.Id);
                b.Property(oi => oi.Quantity).IsRequired();
                b.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
                b.Property(oi => oi.TotalPrice).HasColumnType("decimal(18,2)").IsRequired();

                b.HasOne(oi => oi.Product)
                 .WithMany()
                 .HasForeignKey(oi => oi.ProductId)
                 .OnDelete(DeleteBehavior.Restrict); // don't delete product if order exists

                b.HasIndex(oi => oi.OrderId);
                b.HasIndex(oi => oi.ProductId);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}