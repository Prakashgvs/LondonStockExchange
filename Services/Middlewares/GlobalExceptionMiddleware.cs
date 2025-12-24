using Core.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace Services.Middlewares
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            ErrorResponse response;

            switch (ex)
            {
                case ArgumentException argEx:
                    _logger.LogWarning("Illegal argument: {Message}", argEx.Message);

                    response = new ErrorResponse
                    {
                        Timestamp = DateTime.UtcNow,
                        Status = (int)HttpStatusCode.BadRequest,
                        Error = "Bad Request",
                        Message = argEx.Message
                    };

                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                default:
                    _logger.LogError(ex, "Unexpected error occurred");

                    response = new ErrorResponse
                    {
                        Timestamp = DateTime.UtcNow,
                        Status = (int)HttpStatusCode.InternalServerError,
                        Error = "Internal Server Error",
                        Message = "An unexpected error occurred. Please try again later."
                    };

                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
