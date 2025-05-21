namespace CSharpApp.Application.Categories
{
    /// <inheritdoc cref="ICategoriesService"/>
    public class CategoriesService : BaseService, ICategoriesService
    {
        private readonly RestApiSettings _restApiOptions;

        public CategoriesService(ILogger<CategoriesService> logger, IHttpClientFactory httpClientFactory, IOptions<RestApiSettings> restApiOptions) : base(logger, httpClientFactory)
        {
            _restApiOptions = restApiOptions.Value;
        }

        public async Task<IReadOnlyCollection<Category>> GetCategories()
        {
            var categories = await SendGetRequestAsync<List<Category>>(_restApiOptions.Categories!, nameof(Category));
            return categories.AsReadOnly();
        }

        public async Task<Category> GetCategory(int id)
        {
            var url = $"{_restApiOptions.Categories}/{id}";
            return await SendGetRequestAsync<Category>(url, nameof(Category));
        }

        public async Task<int?> CreateCategory(Category requestData)
        {
            var url = _restApiOptions.Categories!;
            var category = await SendPostRequestAsync<Category, Category>(url, nameof(Category), requestData);
            return category.Id;
        }
    }
}
