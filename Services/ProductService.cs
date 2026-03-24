using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using urban_dukan_product_service.Data;
using urban_dukan_product_service.Models;

namespace urban_dukan_product_service.Services
{
    public class ProductService : IProductService
    {
        private readonly UrbanDukanProductDbContext _db;

        public ProductService(UrbanDukanProductDbContext db)
        {
            _db = db;
        }

        public async Task<ProductsResponse?> GetProductsAsync(int limit = 30, int skip = 0, string? search = null, CancellationToken cancellationToken = default)
        {
            var query = _db.Products.Include(p => p.Images).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim();
                query = query.Where(p => p.Title.Contains(s) || p.Description.Contains(s) || p.Brand.Contains(s) || p.Category.Contains(s));
            }

            var total = await query.CountAsync(cancellationToken);
            var items = await query.OrderBy(p => p.Id).Skip(skip).Take(limit).ToListAsync(cancellationToken);

            return new ProductsResponse
            {
                Products = items,
                Total = total,
                Skip = skip,
                Limit = limit
            };
        }

        public async Task<Product?> GetProductByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _db.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        }
    }
}