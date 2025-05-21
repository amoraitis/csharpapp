using System.Net;
using CSharpApp.Application.Products;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace CSharpApp.Tests.Services;

public class ProductsServiceTests
{
    private ProductsService CreateService(HttpResponseMessage response)
    {
        var restApiSettings = new RestApiSettings { Products = "http://fakeapi/products" };
        var optionsMock = new Mock<IOptions<RestApiSettings>>();
        optionsMock.Setup(o => o.Value).Returns(restApiSettings);

        var loggerMock = new Mock<ILogger<ProductsService>>();

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response);

        var httpClient = new HttpClient(handlerMock.Object);
        var httpClientFactoryMock = new Mock<IHttpClientFactory>();
        httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        return new ProductsService(optionsMock.Object, loggerMock.Object, httpClientFactoryMock.Object);
    }

    [Fact]
    public async Task GetProducts_ReturnsProducts()
    {
        var products = new List<Product> { new Product { Id = 1, Title = "Test" } };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(products), System.Text.Encoding.UTF8, "application/json")
        };
        var service = CreateService(response);

        var result = await service.GetProducts();

        Assert.Single(result);
        Assert.Equal(1, result.First().Id);
    }

    [Fact]
    public async Task GetProduct_ReturnsProduct()
    {
        var product = new Product { Id = 2, Title = "Single" };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(product), System.Text.Encoding.UTF8, "application/json")
        };
        var service = CreateService(response);

        var result = await service.GetProduct(2);

        Assert.Equal(2, result.Id);
        Assert.Equal("Single", result.Title);
    }

    [Fact]
    public async Task CreateProduct_ReturnsProductId()
    {
        var product = new Product { Id = 3, Title = "Created" };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(product), System.Text.Encoding.UTF8, "application/json")
        };
        var service = CreateService(response);

        var result = await service.CreateProduct(new Product { Title = "Created" });

        Assert.Equal(3, result);
    }
}