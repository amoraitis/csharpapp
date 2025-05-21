using System.Net;
using System.Text;
using System.Text.Json;
using CSharpApp.Application.Auth;
using CSharpApp.Core.Dtos;
using CSharpApp.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace CSharpApp.Tests.Services
{
    public class JwtAuthServiceTests
    {
        private static string CreateJwtWithExp(DateTime expiryUtc)
        {
            var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}")).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var payloadObj = new { exp = new DateTimeOffset(expiryUtc).ToUnixTimeSeconds() };
            var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payloadObj))).TrimEnd('=').Replace('+', '-').Replace('/', '_');
            return $"{header}.{payload}.signature";
        }

        private JwtAuthService CreateService(HttpResponseMessage loginResponse, HttpResponseMessage? refreshResponse = null)
        {
            var restApiSettings = new RestApiSettings
            {
                BaseUrl = "http://fakeapi",
                Auth = "/auth/login",
                Username = "user",
                Password = "pass"
            };
            var optionsMock = new Mock<IOptions<RestApiSettings>>();
            optionsMock.Setup(o => o.Value).Returns(restApiSettings);

            var loggerMock = new Mock<ILogger<JwtAuthService>>();

            var handlerMock = new Mock<HttpMessageHandler>();
            var sequence = handlerMock.Protected().SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
            sequence.ReturnsAsync(loginResponse);
            if (refreshResponse != null)
                sequence.ReturnsAsync(refreshResponse);

            var httpClient = new HttpClient(handlerMock.Object);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

            return new JwtAuthService(httpClientFactoryMock.Object, optionsMock.Object, loggerMock.Object);
        }

        [Fact]
        public async Task GetAccessTokenAsync_PerformsLoginAndReturnsToken()
        {
            var expiry = DateTime.UtcNow.AddMinutes(10);
            var jwt = CreateJwtWithExp(expiry);
            var tokenResponse = new JwtTokenResponse
            {
                AccessToken = jwt,
                RefreshToken = "refresh"
            };
            var loginResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
            };

            var service = CreateService(loginResponse);

            var token = await service.GetAccessTokenAsync();

            Assert.Equal(jwt, token);
        }

        [Fact]
        public async Task GetAccessTokenAsync_UsesCachedTokenIfNotExpired()
        {
            var expiry = DateTime.UtcNow.AddMinutes(10);
            var jwt = CreateJwtWithExp(expiry);
            var tokenResponse = new JwtTokenResponse
            {
                AccessToken = jwt,
                RefreshToken = "refresh"
            };
            var loginResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json")
            };

            var service = CreateService(loginResponse);

            // First call triggers login
            var token1 = await service.GetAccessTokenAsync();
            // Second call should use cached token (no new HTTP call)
            var token2 = await service.GetAccessTokenAsync();

            Assert.Equal(token1, token2);
        }

        [Fact]
        public async Task GetAccessTokenAsync_RefreshesTokenIfExpired()
        {
            var expiredJwt = CreateJwtWithExp(DateTime.UtcNow.AddSeconds(-10));
            var validJwt = CreateJwtWithExp(DateTime.UtcNow.AddMinutes(10));
            var loginResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new JwtTokenResponse
                {
                    AccessToken = expiredJwt,
                    RefreshToken = "refresh"
                }), Encoding.UTF8, "application/json")
            };
            var refreshResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new JwtTokenResponse
                {
                    AccessToken = validJwt,
                    RefreshToken = "refresh2"
                }), Encoding.UTF8, "application/json")
            };

            var service = CreateService(loginResponse, refreshResponse);

            // First call triggers login (expired token)
            var token1 = await service.GetAccessTokenAsync();
            // Second call triggers refresh (valid token)
            var token2 = await service.GetAccessTokenAsync();

            Assert.NotEqual(token1, token2);
            Assert.Equal(validJwt, token2);
        }

        [Fact]
        public async Task GetAccessTokenAsync_ThrowsIfLoginFails()
        {
            var loginResponse = new HttpResponseMessage(HttpStatusCode.BadRequest);

            var service = CreateService(loginResponse);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.GetAccessTokenAsync());
        }

        [Fact]
        public async Task GetAccessTokenAsync_ThrowsIfTokenResponseIsInvalid()
        {
            var loginResponse = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            var service = CreateService(loginResponse);

            var ex = await Assert.ThrowsAnyAsync<Exception>(() => service.GetAccessTokenAsync());
            Assert.True(
                ex is InvalidOperationException || ex is NullReferenceException,
                $"Expected InvalidOperationException or NullReferenceException, got {ex.GetType()}"
            );
        }
    }
}
