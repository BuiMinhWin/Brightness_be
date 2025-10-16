using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace PersonalCoachAPI.Presentation.Middleware
{
    public class InternalApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InternalApiKeyMiddleware> _logger;
        private const string InternalApiKeyHeader = "X-Internal-Service-Key";

        public InternalApiKeyMiddleware(
            RequestDelegate next,
            IConfiguration configuration,
            ILogger<InternalApiKeyMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip validation for health checks and swagger
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            if (path.Contains("/health") || path.Contains("/swagger"))
            {
                await _next(context);
                return;
            }

            // Check if request has internal API key header
            if (!context.Request.Headers.TryGetValue(InternalApiKeyHeader, out var providedKey))
            {
                // No internal key provided - continue normally (will be handled by JWT auth)
                await _next(context);
                return;
            }

            // If internal key is provided, validate it
            var expectedKey = _configuration["INTERNAL_API_KEY"];
            
            if (string.IsNullOrEmpty(expectedKey))
            {
                _logger.LogWarning("INTERNAL_API_KEY not configured but internal service request received");
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync("Internal service authentication not configured");
                return;
            }

            if (providedKey != expectedKey)
            {
                _logger.LogWarning(
                    "Invalid internal API key provided from {RemoteIp} for path {Path}",
                    context.Connection.RemoteIpAddress,
                    context.Request.Path);
                
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Forbidden: Invalid internal service key");
                return;
            }

            // Valid internal API key - continue
            _logger.LogDebug("Valid internal service request for path {Path}", context.Request.Path);
            await _next(context);
        }
    }
}

