namespace VTTGROUP.API.Middlewares
{
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        public ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra chỉ áp dụng với các route API
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                if (!context.Request.Headers.TryGetValue("X-Api-Key", out var apiKey))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("🚫 Thiếu API Key.");
                    return;
                }

                var validKey = _configuration["ApiSecurity:ApiKey"];
                if (apiKey != validKey)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("❌ API Key không hợp lệ.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
