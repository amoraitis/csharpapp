namespace CSharpApp.Application.Products;

public class ProductsService : IProductsService
{
    private readonly RestApiSettings _restApiSettings;
    private readonly ILogger<ProductsService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public ProductsService(IOptions<RestApiSettings> restApiSettings, 
        ILogger<ProductsService> logger, IHttpClientFactory httpClientFactory)
    {
        _restApiSettings = restApiSettings.Value;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IReadOnlyCollection<Product>> GetProducts()
    {
        using var httpClient = _httpClientFactory.CreateClient(nameof(Product));
        using var response = await httpClient.GetAsync(_restApiSettings.Products);
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var res = JsonSerializer.Deserialize<List<Product>>(content);
        
        if (res is null)
        {
            _logger.LogError("Failed to deserialize response content");
            throw new InvalidOperationException("Failed to deserialize response content");
        }

        return res.AsReadOnly();
    }
}