using System.Collections.Generic;

namespace urban_dukan_product_service.Models
{
    public class Product
    {
        // Keep Id consistent with external source
        public int Id { get; set; }
        public string Title { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal Price { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal Rating { get; set; }
        public int Stock { get; set; }

        // Allow Brand to be null because external data may omit it
        public string? Brand { get; set; }

        public string Category { get; set; } = default!;
        public string Thumbnail { get; set; } = default!;

        // Navigation: images are stored in a separate table
        public List<ProductImage> Images { get; set; } = new();
    }
}