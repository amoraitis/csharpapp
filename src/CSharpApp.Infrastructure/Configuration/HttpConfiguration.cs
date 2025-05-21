using CSharpApp.Core.Dtos;
using CSharpApp.Infrastructure.Handlers;
using Microsoft.Extensions.Options;

namespace CSharpApp.Infrastructure.Configuration;

public static class HttpConfiguration
{
    public static IServiceCollection AddHttpConfiguration(this IServiceCollection services)
    {
        services.AddTransient<AuthHeaderHandler>();
        services
            .AddHttpClient(nameof(Product), (serviceProvider, httpClient) =>
            {
                var httpClientSettings = serviceProvider.GetRequiredService<IOptions<HttpClientSettings>>().Value;
                var restApiSettings = serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;

                httpClient.BaseAddress = new Uri(restApiSettings.BaseUrl!);
                httpClient.Timeout = TimeSpan.FromSeconds(httpClientSettings.LifeTime);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddHttpMessageHandler<AuthHeaderHandler>();

        services.AddHttpClient(nameof(Category), (serviceProvider, httpClient) =>
        {
            var httpClientSettings = serviceProvider.GetRequiredService<IOptions<HttpClientSettings>>().Value;
            var restApiSettings = serviceProvider.GetRequiredService<IOptions<RestApiSettings>>().Value;
            httpClient.BaseAddress = new Uri(restApiSettings.BaseUrl!);
            httpClient.Timeout = TimeSpan.FromSeconds(httpClientSettings.LifeTime);
            httpClient.DefaultRequestHeaders.Accept.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        })
        .AddHttpMessageHandler<AuthHeaderHandler>();
        return services;
    }
}