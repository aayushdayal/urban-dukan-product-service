using System.Threading;
using System.Threading.Tasks;
using urban_dukan_product_service.Models;

namespace urban_dukan_product_service.Services
{
    public interface IProductService
    {
        Task<ProductsResponse?> GetProductsAsync(int limit = 30, int skip = 0, string? search = null, CancellationToken cancellationToken = default);
        Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}