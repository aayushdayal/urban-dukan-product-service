using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace urban_dukan_product_service.Dtos
{
    // Matches index fields: id, title, description, price, brand, category
    public class ProductSearchDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = null!;

        [JsonPropertyName("title")]
        public string Title { get; set; } = null!;

        [JsonPropertyName("description")]
        public string Description { get; set; } = null!;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("brand")]
        public string? Brand { get; set; } = null;

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;

        // Added: thumbnail returned by the index so controller can map it when DB record is absent
        [JsonPropertyName("thumbnail")]
        public string? Thumbnail { get; set; } = null!;

        // Added missing fields present in the index
        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonPropertyName("rating")]
        public decimal Rating { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; }

        [JsonPropertyName("images")]
        public List<string>? Images { get; set; } = new();
    }
}