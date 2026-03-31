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

        public DbSet<Product> Products => Set<Product>();
        public DbSet<ProductImage> ProductImages => Set<ProductImage>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>(b =>
            {
                b.HasKey(p => p.Id);

                // incoming IDs come from the external provider
                b.Property(p => p.Id).ValueGeneratedNever();

                b.Property(p => p.Title).HasMaxLength(250).IsRequired();
                b.Property(p => p.Description).HasMaxLength(2000);

                // Make Brand optional (nullable) in the schema
                b.Property(p => p.Brand).HasMaxLength(250).IsRequired(false);

                b.Property(p => p.Price).HasColumnType("decimal(18,2)");
                b.Property(p => p.DiscountPercentage).HasColumnType("decimal(5,2)");
                b.Property(p => p.Rating).HasColumnType("decimal(3,2)");
                b.HasMany(p => p.Images).WithOne(i => i.Product!).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProductImage>(b =>
            {
                b.HasKey(i => i.Id);
                b.Property(i => i.Url).HasMaxLength(2000).IsRequired();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}