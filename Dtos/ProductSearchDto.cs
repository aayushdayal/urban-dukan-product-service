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
        public string Brand { get; set; } = null;

        [JsonPropertyName("category")]
        public string Category { get; set; } = null!;
    }
}