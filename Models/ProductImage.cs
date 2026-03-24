namespace urban_dukan_product_service.Models
{
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string Url { get; set; } = default!;
        public Product? Product { get; set; }
    }
}