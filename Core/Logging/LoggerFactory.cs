using Core.Interfaces;
using Core.Logging;
using Microsoft.Extensions.Configuration;

namespace Core.Logging
{
    public static class LoggerFactory
    {
        public static IApplicationLogger CreateLogger(string loggerProvider, IConfiguration configuration)
        {
            return loggerProvider.ToLower() switch
            {
                "serilog" => new SerilogLogger(configuration),
                _ => new SerilogLogger(configuration)
            };
        }
    }
}
