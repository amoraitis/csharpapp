using System.Text;

namespace CSharpApp.Application
{
    public class BaseService
    {
        private readonly ILogger<BaseService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        internal BaseService(ILogger<BaseService> logger, IHttpClientFactory httpClientFactory)
        {
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        protected async Task<T> SendGetRequestAsync<T>(string url, string clientName)
        {
            using var httpClient = _httpClientFactory.CreateClient(clientName);
            using var response = await httpClient.GetAsync(url);
            // TODO: Handle not found
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<T>(content);

            if (result is null)
            {
                _logger.LogError("Failed to deserialize response content for type {Type}", typeof(T).Name);
                throw new InvalidOperationException("Failed to deserialize response content");
            }

            return result;
        }

        protected async Task<TResponse> SendPostRequestAsync<TRequest, TResponse>(string url, string clientName, TRequest requestData)
        {
            using var httpClient = _httpClientFactory.CreateClient(clientName);
            using var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json"
            );

            using var response = await httpClient.PostAsync(url, jsonContent);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<TResponse>(content);

            if (result is null)
            {
                _logger.LogError("Failed to deserialize response content for type {Type}", typeof(TResponse).Name);
                throw new InvalidOperationException("Failed to deserialize response content");
            }

            return result;
        }
    }
}
