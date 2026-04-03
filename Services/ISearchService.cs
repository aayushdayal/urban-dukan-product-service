using urban_dukan_product_service.Dtos;

namespace urban_dukan_product_service.Services
{
    public interface ISearchService
    {
        Task UploadProductsAsync(List<ProductSearchDto> products);
        //  Task<List<ProductSearchDto>> SearchAsync(string query, string? category = null, string? brand = null, double? minPrice = null, double? maxPrice = null, string? sortBy = null);
        Task<List<ProductSearchDto>> SearchAsync(string query, string? category = null, string? brand = null, double? minPrice = null, double? maxPrice = null, string? sortBy = null, int skip = 0, int size = 100);
        Task DeleteProductAsync(string id);
    }
}