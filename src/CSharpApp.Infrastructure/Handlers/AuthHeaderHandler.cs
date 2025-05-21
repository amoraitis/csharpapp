using System.Net.Http.Headers;

namespace CSharpApp.Infrastructure.Handlers
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IJwtAuthService _jwtAuthService;

        public AuthHeaderHandler(IJwtAuthService jwtAuthService)
        {
            _jwtAuthService = jwtAuthService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _jwtAuthService.GetAccessTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return await base.SendAsync(request, cancellationToken);
        }
    }
}
