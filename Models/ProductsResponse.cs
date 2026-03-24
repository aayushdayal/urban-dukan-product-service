using System.Collections.Generic;

namespace urban_dukan_product_service.Models
{
    public class ProductsResponse
    {
        public List<Product> Products { get; set; } = new();
        public int Total { get; set; }
        public int Skip { get; set; }
        public int Limit { get; set; }
    }
}