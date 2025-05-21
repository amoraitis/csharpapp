using System.Text;

namespace CSharpApp.Application.Auth
{
    public class JwtAuthService : BaseService, IJwtAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly RestApiSettings _settings;
        private readonly ILogger<JwtAuthService> _logger;
        private string? _accessToken;
        private string? _refreshToken;
        private DateTime _accessTokenExpiry = DateTime.MinValue;
        private readonly object _lock = new();

        public JwtAuthService(
            IHttpClientFactory httpClientFactory,
            IOptions<RestApiSettings> options,
            ILogger<JwtAuthService> logger) : base(logger, httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            _settings = options.Value;
            _logger = logger;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            lock (_lock)
            {
                if (!string.IsNullOrEmpty(_accessToken) && _accessTokenExpiry > DateTime.UtcNow.AddMinutes(1))
                    return _accessToken;
            }

            if (string.IsNullOrEmpty(_refreshToken))
            {
                await LoginAsync();
            }
            else
            {
                await RefreshTokenAsync();
            }

            lock (_lock)
            {
                if (string.IsNullOrEmpty(_accessToken))
                    throw new InvalidOperationException("Failed to obtain access token.");
                return _accessToken;
            }
        }

        private async Task LoginAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_settings.BaseUrl?.TrimEnd('/')}{_settings.Auth}";
            var loginRequest = new JwtLoginRequest
            {
                Email = _settings.Username!,
                Password = _settings.Password!
            };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JwtTokenResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (tokenResponse is null)
                throw new InvalidOperationException("Failed to deserialize JWT token response.");

            lock (_lock)
            {
                _accessToken = tokenResponse.AccessToken;
                _refreshToken = tokenResponse.RefreshToken;
                _accessTokenExpiry = ParseJwtExpiry(_accessToken);
            }
        }

        private async Task RefreshTokenAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var url = $"{_settings.BaseUrl?.TrimEnd('/')}/auth/refresh-token";
            var refreshRequest = new JwtRefreshRequest { RefreshToken = _refreshToken! };
            var content = new StringContent(JsonSerializer.Serialize(refreshRequest), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, content);
            if (!response.IsSuccessStatusCode)
            {
                // fallback to login if refresh fails
                await LoginAsync();
                return;
            }
            var json = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<JwtTokenResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (tokenResponse is null)
                throw new InvalidOperationException("Failed to deserialize JWT token response.");

            lock (_lock)
            {
                _accessToken = tokenResponse.AccessToken;
                _refreshToken = tokenResponse.RefreshToken;
                _accessTokenExpiry = ParseJwtExpiry(_accessToken);
            }
        }

        private static DateTime ParseJwtExpiry(string jwt)
        {
            // JWT format: header.payload.signature
            var parts = jwt.Split('.');
            if (parts.Length != 3)
                return DateTime.UtcNow.AddMinutes(1);

            var payload = parts[1];
            var json = Encoding.UTF8.GetString(Convert.FromBase64String(PadBase64(payload)));
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("exp", out var exp))
            {
                var seconds = exp.GetInt64();
                var date = DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
                return date;
            }
            return DateTime.UtcNow.AddMinutes(1);
        }

        private static string PadBase64(string base64)
        {
            // Pad base64 string if needed
            int padding = 4 - (base64.Length % 4);
            if (padding < 4)
                base64 += new string('=', padding);
            return base64.Replace('-', '+').Replace('_', '/');
        }
    }
}
