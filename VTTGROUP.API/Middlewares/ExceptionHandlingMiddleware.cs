using System.Net;
using System.Text.Json;

namespace VTTGROUP.API.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context); // tiếp tục pipeline
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unhandled Exception: {Message}", ex.Message.ToString());
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;

            var result = JsonSerializer.Serialize(new
            {
                success = false,
                error = "Có lỗi xảy ra trong hệ thống. Vui lòng thử lại sau.",
                detail = exception.Message.ToString() // Có thể ẩn đi ở production
            });

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            return context.Response.WriteAsync(result);
        }
    }
}
