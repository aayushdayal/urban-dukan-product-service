using System.Threading;
using System.Threading.Tasks;

namespace urban_dukan_product_service.Services
{
    public interface IDbSeeder
    {
        Task SeedAsync(CancellationToken cancellationToken = default);
    }
}