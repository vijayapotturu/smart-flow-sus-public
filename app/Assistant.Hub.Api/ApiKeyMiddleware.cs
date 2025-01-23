namespace Assistants.API
{
    public class ApiKeyMiddleware
    {
        private static readonly string[] unsecuredPaths = new string[] { "/health", "/ready", "/swagger" };

        private readonly RequestDelegate _next;
        private const string ApiKeyHeaderName = "X-Api-Key";
        private readonly string _apiKey;

        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _apiKey = configuration.GetValue<string>("ApiKey");
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (unsecuredPaths.Any(path => context.Request.Path.StartsWithSegments(path)))
            {
                await _next(context);
                return;
            }

            if (!context.Request.Headers.TryGetValue(ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("API Key was not provided.");
                return;
            }

            if (!string.Equals(extractedApiKey, _apiKey, StringComparison.Ordinal))
            {
                context.Response.StatusCode = 403;
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await _next(context);
        }
    }
}
