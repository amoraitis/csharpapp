namespace CSharpApp.Core.Dtos
{
    public class JwtLoginRequest
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = default!;
        [JsonPropertyName("password")]
        public string Password { get; set; } = default!;
    }

    public class JwtTokenResponse
    {
        [JsonPropertyName("Access_Token")]
        public string AccessToken { get; set; } = default!;
        
        [JsonPropertyName("RefreshToken")]
        public string RefreshToken { get; set; } = default!;
    }

    public class JwtRefreshRequest
    {
        public string RefreshToken { get; set; } = default!;
    }
}
