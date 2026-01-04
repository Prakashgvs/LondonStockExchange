using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Core.Logging
{
    public class SerilogLogger : IApplicationLogger
    {
        private readonly ILogger _logger;

        public SerilogLogger(IConfiguration configuration)
        {
            _logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();
        }

        public void LogInformation(string message, params object[] args)
        {
            _logger.Information(message, args);
        }

        public void LogWarning(string message, params object[] args)
        {
            _logger.Warning(message, args);
        }

        public void LogError(string message, Exception? exception = null, params object[] args)
        {
            if (exception != null)
                _logger.Error(exception, message, args);
            else
                _logger.Error(message, args);
        }

        public void LogDebug(string message, params object[] args)
        {
            _logger.Debug(message, args);
        }
    }
}
