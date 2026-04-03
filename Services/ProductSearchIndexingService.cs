namespace urban_dukan_product_service.Services
{
    public class ProductSearchIndexingService
    {
        private readonly IProductService _productService;
        private readonly ISearchService _searchService;
        private readonly ILogger<ProductSearchIndexingService> _logger;

        public ProductSearchIndexingService(
            IProductService productService,
            ISearchService searchService,
            ILogger<ProductSearchIndexingService> logger)
        {
            _productService = productService;
            _searchService = searchService;
            _logger = logger;
        }

        public async Task ReindexAllProductsAsync(CancellationToken cancellationToken)
        {
            int skip = 0;
            int take = 100;

            while (!cancellationToken.IsCancellationRequested)
            {
                var products = await _productService
                    .GetProductsForIndexingAsync(skip, take, cancellationToken);

                if (products == null || !products.Any())
                    break;

                _logger.LogInformation("Indexing batch Skip={Skip}, Count={Count}", skip, products.Count);
//Uncomment when you want to reindex all products. Be cautious as this will overwrite existing index data.
                //      await _searchService.UploadProductsAsync(products);

                skip += take;
            }
        }
    }
}
