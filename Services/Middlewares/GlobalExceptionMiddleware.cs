using Core.Entities;
using Core.Exceptions;
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
            var correlationId = Guid.NewGuid().ToString();
            context.Response.Headers.Append("X-Correlation-Id", correlationId);

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, correlationId);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception ex, string correlationId)
        {
            var statusCode = GetStatusCode(ex);
            var errorType = GetErrorType(ex);
            var message = GetErrorMessage(ex, statusCode);

            LogException(ex, correlationId, errorType);

            var response = new ErrorResponse
            {
                Timestamp = DateTime.UtcNow,
                Status = (int)statusCode,
                Error = errorType,
                Message = message,
                CorrelationId = correlationId
            };

            context.Response.StatusCode = (int)statusCode;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private HttpStatusCode GetStatusCode(Exception ex)
        {
            return ex switch
            {
                ValidationException => HttpStatusCode.BadRequest,
                ArgumentException => HttpStatusCode.BadRequest,
                UnauthorizedException => HttpStatusCode.Unauthorized,
                _ => HttpStatusCode.InternalServerError
            };
        }

        private string GetErrorType(Exception ex)
        {
            return ex switch
            {
                ValidationException => "Validation Error",
                ArgumentException => "Bad Request",
                UnauthorizedException => "Unauthorized",
                _ => "Internal Server Error"
            };
        }

        private string GetErrorMessage(Exception ex, HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.InternalServerError
                ? "An unexpected error occurred. Please try again later."
                : ex.Message;
        }

        private void LogException(Exception ex, string correlationId, string errorType)
        {
            if (ex is ValidationException or ArgumentException or UnauthorizedException)
            {
                _logger.LogWarning("{ErrorType} - CorrelationId: {CorrelationId}, Message: {Message}",
                    errorType, correlationId, ex.Message);
            }
            else
            {
                _logger.LogError(ex, "Unexpected error - CorrelationId: {CorrelationId}", correlationId);
            }
        }
    }
}
