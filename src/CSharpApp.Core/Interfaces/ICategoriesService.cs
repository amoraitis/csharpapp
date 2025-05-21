namespace CSharpApp.Core.Interfaces
{
    /// <summary>
    /// Service responsible for interacting with the external category service.
    /// </summary>
    public interface ICategoriesService
    {
        /// <summary>
        /// Get all categories.
        /// </summary>
        /// <returns>A collection with all available categories.</returns>
        Task<IReadOnlyCollection<Category>> GetCategories();

        /// <summary>
        /// Get a category by id.
        /// </summary>
        /// <param name="id">The id to search a category with.</param>
        /// <returns>The <see cref="Category"/>.</returns>
        Task<Category> GetCategory(int id);

        /// <summary>
        /// Create a new category.
        /// </summary>
        /// <param name="requestData">The category data to create for.</param>
        /// <returns>The id of the category created.</returns>
        Task<int?> CreateCategory(Category requestData);
    }
}
