namespace CSharpApp.Application.Products;

/// <inheritdoc cref="IProductsService"/>
public class ProductsService : BaseService, IProductsService
{
    private readonly RestApiSettings _restApiSettings;

    public ProductsService(
        IOptions<RestApiSettings> restApiSettings,
        ILogger<ProductsService> logger,
        IHttpClientFactory httpClientFactory)
        : base(logger, httpClientFactory)
    {
        _restApiSettings = restApiSettings.Value;
    }

    public async Task<IReadOnlyCollection<Product>> GetProducts()
    {
        var products = await SendGetRequestAsync<List<Product>>(_restApiSettings.Products!, nameof(Product));
        return products.AsReadOnly();
    }

    public async Task<Product> GetProduct(int id)
    {
        var url = $"{_restApiSettings.Products}/{id}";
        return await SendGetRequestAsync<Product>(url, nameof(Product));
    }

    public async Task<int?> CreateProduct(Product requestData)
    {
        var url = _restApiSettings.Products!;
        var product = await SendPostRequestAsync<Product, Product>(url, nameof(Product), requestData);

        return product.Id;
    }
}