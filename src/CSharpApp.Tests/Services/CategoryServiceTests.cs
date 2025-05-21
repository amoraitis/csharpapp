using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using CSharpApp.Application.Categories;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace CSharpApp.Tests.Services
{
    public class CategoryServiceTests
    {
        private CategoriesService CreateService(HttpResponseMessage response)
        {
            var restApiSettings = new RestApiSettings { Categories = "http://fakeapi/categories" };
            var optionsMock = new Mock<IOptions<RestApiSettings>>();
            optionsMock.Setup(o => o.Value).Returns(restApiSettings);

            var loggerMock = new Mock<ILogger<CategoriesService>>();

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

            return new CategoriesService(loggerMock.Object, httpClientFactoryMock.Object, optionsMock.Object);
        }

        [Fact]
        public async Task GetCategories_ReturnsCategories()
        {
            var categories = new List<Category>
            {
                new Category { Id = 1, Name = "Cat1" },
                new Category { Id = 2, Name = "Cat2" }
            };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(categories), Encoding.UTF8, "application/json")
            };
            var service = CreateService(response);

            var result = await service.GetCategories();

            Assert.Equal(2, result.Count);
            Assert.Contains(result, c => c.Name == "Cat1");
            Assert.Contains(result, c => c.Name == "Cat2");
        }

        [Fact]
        public async Task GetCategory_ReturnsCategory()
        {
            var category = new Category { Id = 10, Name = "SingleCat" };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json")
            };
            var service = CreateService(response);

            var result = await service.GetCategory(10);

            Assert.Equal(10, result.Id);
            Assert.Equal("SingleCat", result.Name);
        }

        [Fact]
        public async Task CreateCategory_ReturnsCategoryId()
        {
            var category = new Category { Id = 99, Name = "CreatedCat" };
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(category), Encoding.UTF8, "application/json")
            };
            var service = CreateService(response);

            var result = await service.CreateCategory(new Category { Name = "CreatedCat" });

            Assert.Equal(99, result);
        }
    }
}
