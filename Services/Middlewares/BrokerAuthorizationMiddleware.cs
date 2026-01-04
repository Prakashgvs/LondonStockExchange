using Core.Exceptions;
using Core.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Services.Middlewares
{
    public class BrokerAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<BrokerAuthorizationMiddleware> _logger;

        public BrokerAuthorizationMiddleware(
            RequestDelegate next,
            ILogger<BrokerAuthorizationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, IBrokerRepository brokerRepository)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (ShouldSkipAuthorization(path))
            {
                await _next(context);
                return;
            }

            var brokerId = context.Request.Headers["X-Broker-Id"].FirstOrDefault();

            if (string.IsNullOrEmpty(brokerId))
                throw new UnauthorizedException("Broker ID is required");

            var isActive = await brokerRepository.IsBrokerActiveAsync(brokerId);

            if (!isActive)
                throw new UnauthorizedException("Invalid or inactive broker account");

            context.Items["BrokerId"] = brokerId;

            _logger.LogInformation("Broker {BrokerId} authorized", brokerId);

            await _next(context);
        }

        private bool ShouldSkipAuthorization(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            return path.StartsWith("/swagger");
        }
    }
}
