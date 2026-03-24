using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using urban_dukan_product_service.Data;
using urban_dukan_product_service.Dtos;
using urban_dukan_product_service.Models;

namespace urban_dukan_product_service.Services
{
    public class DbSeeder : IDbSeeder
    {
        private readonly UrbanDukanProductDbContext _db;
        private readonly HttpClient _http;
        private readonly ILogger<DbSeeder> _logger;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        public DbSeeder(UrbanDukanProductDbContext db, IHttpClientFactory httpFactory, ILogger<DbSeeder> logger)
        {
            _db = db;
            _http = httpFactory.CreateClient("external");
            _logger = logger;
        }

        public async Task SeedAsync(CancellationToken cancellationToken = default)
        {
            if (await _db.Products.AnyAsync(cancellationToken))
            {
                _logger.LogInformation("DbSeeder: database already contains products; skipping seeding.");
                return;
            }

            _logger.LogInformation("DbSeeder: beginning seed from external source.");

            try
            {
                using var resp = await _http.GetAsync("/products?limit=1000", cancellationToken);
                if (!resp.IsSuccessStatusCode)
                {
                    _logger.LogError("DbSeeder: external request failed. StatusCode={StatusCode}", resp.StatusCode);
                    return;
                }

                var external = await resp.Content.ReadFromJsonAsync<ExternalProductsResponseDto>(_jsonOptions, cancellationToken);
                if (external?.Products == null || external.Products.Count == 0)
                {
                    _logger.LogWarning("DbSeeder: external source returned no products.");
                    return;
                }

                foreach (var ep in external.Products)
                {
                    if (await _db.Products.AnyAsync(p => p.Id == ep.Id, cancellationToken))
                    {
                        _logger.LogDebug("DbSeeder: product {Id} already exists; skipping.", ep.Id);
                        continue;
                    }

                    var product = new Product
                    {
                        Id = ep.Id,
                        Title = ep.Title,
                        Description = ep.Description,
                        Price = ep.Price,
                        DiscountPercentage = ep.DiscountPercentage,
                        Rating = ep.Rating,
                        Stock = ep.Stock,
                        Brand = ep.Brand,
                        Category = ep.Category,
                        Thumbnail = ep.Thumbnail
                    };

                    foreach (var img in ep.Images)
                    {
                        product.Images.Add(new ProductImage { Url = img });
                    }

                    _db.Products.Add(product);
                }

                var saved = await _db.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("DbSeeder: finished seeding. Saved changes count: {SavedCount}", saved);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DbSeeder: seeding failed with exception.");
                throw; // rethrow so startup shows the failure
            }
        }
    }
}