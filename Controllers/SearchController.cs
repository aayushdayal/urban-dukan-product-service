using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using urban_dukan_product_service.Dtos;
using urban_dukan_product_service.Services;
using urban_dukan_product_service.Models;

namespace urban_dukan_product_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _search;
        private readonly IProductService _productService;
        private readonly ILogger<SearchController> _logger;
        private const int MaxLimit = 100;

        public SearchController(ISearchService search, IProductService productService, ILogger<SearchController> logger)
        {
            _search = search;
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Search products in Azure AI Search.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProductsResponse), 200)]
        [ProducesResponseType(502)]
        public async Task<IActionResult> Get([FromQuery(Name = "q")] string? q = null,
                                             [FromQuery] string? category = null,
                                             [FromQuery] string? brand = null,
                                             [FromQuery] double? minPrice = null,
                                             [FromQuery] double? maxPrice = null,
                                             [FromQuery] string? sortBy = null,
                                             [FromQuery] int limit = 30,
                                             [FromQuery] int skip = 0)
        {
            try
            {
                // Basic validation/clamping like ProductsController
                limit = System.Math.Clamp(limit, 1, MaxLimit);
                skip = System.Math.Max(0, skip);

                var searchQuery = string.IsNullOrWhiteSpace(q) ? "*" : q;

                string? orderBy = sortBy?.ToLower() switch
                {
                    "priceasc" => "price asc",
                    "pricedesc" => "price desc",
                    _ => null
                };

                var results = await _search.SearchAsync(
                    searchQuery,
                    category,
                    brand,
                    minPrice,
                    maxPrice,
                    orderBy,
                    skip,
                    limit);

                var ct = HttpContext?.RequestAborted ?? CancellationToken.None;

                // Robust extraction of numeric id from index id string.
                // Some indexes may store ids like "1", "product-1", "products/1" etc.
                var ids = results
                    .Select(r =>
                    {
                        if (int.TryParse(r.Id, out var v)) return v;
                        var m = Regex.Match(r.Id ?? string.Empty, @"\d+");
                        return m.Success ? int.Parse(m.Value) : 0;
                    })
                    .Where(id => id > 0)
                    .Distinct()
                    .ToList();

                List<Product> products;

                if (ids.Count > 0)
                {
                    // Fetch full product records (all properties) using existing product service
                    var dbProducts = await _productService.GetProductsByIdsAsync(ids, ct);
                    var map = dbProducts.ToDictionary(p => p.Id);

                    // Preserve the order from search results and use full DB record when available.
                    products = new List<Product>(results.Count);
                    foreach (var dto in results)
                    {
                        var extractedId = 0;
                        if (!int.TryParse(dto.Id, out extractedId))
                        {
                            var m = Regex.Match(dto.Id ?? string.Empty, @"\d+");
                            if (m.Success) extractedId = int.Parse(m.Value);
                        }

                        if (extractedId > 0 && map.TryGetValue(extractedId, out var full))
                        {
                            // Use the DB entity as-is (contains all properties)
                            products.Add(full);
                        }
                        else
                        {
                            // If DB record missing, fallback to DTO mapping (should be rare)
                            products.Add(new Product
                            {
                                Id = extractedId,
                                Title = dto.Title ?? string.Empty,
                                Description = dto.Description ?? string.Empty,
                                Price = dto.Price,
                                Brand = dto.Brand,
                                Category = dto.Category ?? string.Empty,
                                Thumbnail = dto.Thumbnail ?? string.Empty,
                                DiscountPercentage = dto.DiscountPercentage,
                                Rating = dto.Rating,
                                Stock = dto.Stock,
                                Images = dto.Images?.Select(u => new ProductImage { Url = u }).ToList() ?? new List<ProductImage>()
                            });
                        }
                    }
                }
                else
                {
                    // No numeric ids found in index results - build products from DTOs
                    products = results.Select(dto =>
                    {
                        int parsedId = 0;
                        if (!int.TryParse(dto.Id, out parsedId))
                        {
                            var m = Regex.Match(dto.Id ?? string.Empty, @"\d+");
                            if (m.Success) parsedId = int.Parse(m.Value);
                        }

                        return new Product
                        {
                            Id = parsedId,
                            Title = dto.Title ?? string.Empty,
                            Description = dto.Description ?? string.Empty,
                            Price = dto.Price,
                            Brand = dto.Brand,
                            Category = dto.Category ?? string.Empty,
                            Thumbnail = dto.Thumbnail ?? string.Empty,
                            DiscountPercentage = dto.DiscountPercentage,
                            Rating = dto.Rating,
                            Stock = dto.Stock,
                            Images = dto.Images?.Select(u => new ProductImage { Url = u }).ToList() ?? new List<ProductImage>()
                        };
                    }).ToList();
                }

                var response = new ProductsResponse
                {
                    Products = products,
                    Total = products.Count,
                    Skip = skip,
                    Limit = limit
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Search endpoint failed.");
                return StatusCode(502, "Search provider error.");
            }
        }

        /// <summary>
        /// Upload or update documents into the search index.
        /// </summary>
        [HttpPost("index")]
        [ProducesResponseType(202)]
        [ProducesResponseType(400)]
        [ProducesResponseType(502)]
        public async Task<IActionResult> Index([FromBody] List<ProductSearchDto>? products)
        {
            if (products == null || products.Count == 0)
                return BadRequest("Request must contain a non-empty list of products.");

            try
            {
                await _search.UploadProductsAsync(products).ConfigureAwait(false);
                return Accepted(new { message = "Products indexed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Indexing failed.");
                return StatusCode(502, "Indexing failed.");
            }
        }

        /// <summary>
        /// Delete a document from the index.
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(502)]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest("Id is required.");
            try
            {
                await _search.DeleteProductAsync(id).ConfigureAwait(false);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Delete from index failed for id={Id}.", id);
                return StatusCode(502, "Delete from index failed.");
            }
        }

        [HttpGet("autocomplete")]
        public async Task<IActionResult> Autocomplete([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
                return Ok(new List<string>());

            var results = await _search.GetSuggestionsAsync(query);

            return Ok(results);
        }
    }
}