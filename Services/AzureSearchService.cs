using Azure;
using Azure.Search.Documents;
using System.Globalization;
using urban_dukan_product_service.Dtos;

namespace urban_dukan_product_service.Services
{
    public class AzureSearchService : ISearchService
    {
        private readonly SearchClient _client;
        private readonly ILogger<AzureSearchService> _logger;

        // track one-time warning for missing CreatedDate in Product

        public AzureSearchService(SearchClient client, ILogger<AzureSearchService> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // Existing method — keep for compatibility
        public async Task UploadProductsAsync(List<ProductSearchDto> products)
        {
            if (products == null) throw new ArgumentNullException(nameof(products));

            try
            {
                var response = await _client.UploadDocumentsAsync(products).ConfigureAwait(false);
                var results = response.Value?.Results;
                if (results != null)
                {
                    var failed = results.Where(r => !r.Succeeded).ToList();
                    if (failed.Any())
                    {
                        foreach (var f in failed)
                        {
                            _logger.LogWarning("Failed to upload document key={Key}, error={Error}", f.Key, f.ErrorMessage);
                        }

                        throw new InvalidOperationException("One or more documents failed to upload to Azure Search. Check logs for details.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadProductsAsync failed.");
                throw;
            }
        }

        public async Task<List<ProductSearchDto>> SearchAsync(string query, string? category = null, string? brand = null, double? minPrice = null, double? maxPrice = null, string? sortBy = null, int skip = 0, int size = 100)
        {
            try
            {
                var options = new SearchOptions
                {
                    Size = size,
                    IncludeTotalCount = true,
                    Skip = skip
                };

                var filters = new List<string>();

                static string EscapeForFilter(string value) => value.Replace("'", "''");

                if (!string.IsNullOrWhiteSpace(category))
                    filters.Add($"category eq '{EscapeForFilter(category!)}'");

                if (!string.IsNullOrWhiteSpace(brand))
                    filters.Add($"brand eq '{EscapeForFilter(brand!)}'");

                if (minPrice.HasValue)
                    filters.Add($"price ge {minPrice.Value.ToString(CultureInfo.InvariantCulture)}");

                if (maxPrice.HasValue)
                    filters.Add($"price le {maxPrice.Value.ToString(CultureInfo.InvariantCulture)}");

                if (filters.Count > 0)
                    options.Filter = string.Join(" and ", filters);

                if (!string.IsNullOrWhiteSpace(sortBy))
                    options.OrderBy.Add(sortBy);

                var q = string.IsNullOrWhiteSpace(query) ? "*" : query;

                var results = new List<ProductSearchDto>();
                var response = await _client.SearchAsync<ProductSearchDto>(q, options).ConfigureAwait(false);
                var searchResults = response.Value;

                await foreach (var r in searchResults.GetResultsAsync())
                {
                    if (r.Document != null)
                        results.Add(r.Document);
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SearchAsync failed for query='{Query}'", query);
                throw;
            }
        }

        // Delete by key (id)
        public async Task DeleteProductAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));

            try
            {
                await _client.DeleteDocumentsAsync("id", new[] { id }).ConfigureAwait(false);
            }
            catch (RequestFailedException rfEx) when (rfEx.Status == 404)
            {
                _logger.LogInformation("DeleteProductAsync: document id={Id} not found in index.", id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteProductAsync failed for id={Id}", id);
                throw;
            }
        }

    }
    
}