using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using urban_dukan_product_service.Dtos;
using urban_dukan_product_service.Services;

namespace urban_dukan_product_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchService _search;
        private readonly ILogger<SearchController> _logger;

        public SearchController(ISearchService search, ILogger<SearchController> logger)
        {
            _search = search;
            _logger = logger;
        }

        /// <summary>
        /// Search products in Azure AI Search.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<ProductSearchDto>), 200)]
        [ProducesResponseType(502)]
        public async Task<IActionResult> Get([FromQuery(Name = "q")] string? q = null,
                                             [FromQuery] string? category = null,
                                             [FromQuery] string? brand = null,
                                             [FromQuery] double? minPrice = null,
                                             [FromQuery] double? maxPrice = null,
                                             [FromQuery] string? sortBy = null)
        {
            try
            {
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
                    orderBy);

                return Ok(results);

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
    }
}