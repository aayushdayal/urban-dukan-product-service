namespace urban_dukan_product_service.Services
{
    public class SearchReindexingHostedService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SearchReindexingHostedService> _logger;
        private readonly bool _enabled;

        public SearchReindexingHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<SearchReindexingHostedService> logger,
            IConfiguration config)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;

            _enabled = config.GetValue<bool>("AzureSearch:AutoReindexOnStartup", false);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_enabled)
            {
                _logger.LogInformation("Auto reindex disabled.");
                return;
            }

            try
            {
                _logger.LogInformation("Starting auto reindex...");

                using var scope = _scopeFactory.CreateScope();

                var indexingService =
                    scope.ServiceProvider.GetRequiredService<ProductSearchIndexingService>();

                await indexingService.ReindexAllProductsAsync(stoppingToken);

                _logger.LogInformation("Auto reindex completed.");
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Auto reindex cancelled.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Auto reindex failed.");
            }
        }
    }
}