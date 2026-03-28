using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading;
using System.Threading.Tasks;
using urban_dukan_product_service.Models;
using urban_dukan_product_service.Services;

namespace urban_dukan_product_service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _service;
        private const int MaxLimit = 100;

        public ProductsController(IProductService service)
        {
            _service = service;
        }

        /// <summary>
        /// Get a paged list of products. Optional search will use the provider's search endpoint.
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ProductsResponse), 200)]
        [ProducesResponseType(502)]
        public async Task<IActionResult> Get([FromQuery] int limit = 30, [FromQuery] int skip = 0, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
        {
            // Basic validation/clamping to avoid abuse and accidental negative values
            limit = System.Math.Clamp(limit, 1, MaxLimit);
            skip = System.Math.Max(0, skip);

            var result = await _service.GetProductsAsync(limit, skip, search, cancellationToken);
            if (result == null) return StatusCode(502, "Failed to fetch products from upstream service.");
            return Ok(result);
        }

        /// <summary>
        /// Get product by id.
        /// </summary>
        [Authorize]
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Product), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(502)]
        public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken = default)
        {
            var product = await _service.GetProductByIdAsync(id, cancellationToken);
            if (product == null) return NotFound();
            return Ok(product);
        }
    }
}