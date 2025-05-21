using System.Diagnostics;

namespace CSharpApp.Api.Middleware
{
    /// <summary>
    /// Middleware to log the time taken to process a request.
    /// </summary>
    public class RequestTimingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTimingMiddleware> _logger;

        public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            var elapsedMs = stopwatch.ElapsedMilliseconds;
            var path = context.Request.Path;
            var method = context.Request.Method;
            var statusCode = context.Response.StatusCode;

            _logger.LogInformation("Request {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                method, path, statusCode, elapsedMs);
        }
    }
}
