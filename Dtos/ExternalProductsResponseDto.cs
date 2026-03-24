using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace urban_dukan_product_service.Dtos
{
    public class ExternalProductsResponseDto
    {
        [JsonPropertyName("products")]
        public List<ExternalProductDto> Products { get; set; } = new();
        [JsonPropertyName("total")]
        public int Total { get; set; }
        [JsonPropertyName("skip")]
        public int Skip { get; set; }
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
    }
}