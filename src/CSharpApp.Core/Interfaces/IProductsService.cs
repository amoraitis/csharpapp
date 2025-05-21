namespace CSharpApp.Core.Interfaces;

/// <summary>
/// Service responsible for interacting with the external product service.
/// </summary>
public interface IProductsService
{
    /// <summary>
    /// Get all products.
    /// </summary>
    /// <returns>A collection with all available products.</returns>
    Task<IReadOnlyCollection<Product>> GetProducts();

    /// <summary>
    /// Get a product by id.
    /// </summary>
    /// <param name="id">The id to search a product with.</param>
    /// <returns>The <see cref="Product"/>.</returns>
    Task<Product> GetProduct(int id);

    /// <summary>
    /// Create a new product.
    /// </summary>
    /// <param name="requestData">The product data to create for.</param>
    /// <returns>The id of the product created.</returns>
    Task<int?> CreateProduct(Product requestData);
}