using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace urban_dukan_product_service.Dtos
{
    // DTO to match dummyjson product shape used only for seeding
    public class ExternalProductDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; } = default!;
        [JsonPropertyName("description")]
        public string Description { get; set; } = default!;
        [JsonPropertyName("price")]
        public decimal Price { get; set; }
        [JsonPropertyName("discountPercentage")]
        public decimal DiscountPercentage { get; set; }
        [JsonPropertyName("rating")]
        public decimal Rating { get; set; }
        [JsonPropertyName("stock")]
        public int Stock { get; set; }
        [JsonPropertyName("brand")]
        public string Brand { get; set; } = default!;
        [JsonPropertyName("category")]
        public string Category { get; set; } = default!;
        [JsonPropertyName("thumbnail")]
        public string Thumbnail { get; set; } = default!;
        [JsonPropertyName("images")]
        public List<string> Images { get; set; } = new();
    }
}