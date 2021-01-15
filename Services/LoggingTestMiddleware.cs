using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    public class LoggingTestMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingTestMiddleware(RequestDelegate next) => _next = next;

        public Task InvokeAsync(
            HttpContext context,
            ILogger<LoggingTestMiddleware> logger,
            TelemetryClient telemetryClient,
            IClock clock)
        {
            logger.LogTrace("This is a trace at {0} UTC.", clock.UtcNow);
            logger.LogDebug("This is a debug message {0} UTC.", clock.UtcNow);
            logger.LogInformation("This is an info message {0} UTC.", clock.UtcNow);
            logger.LogWarning("This is a warning {0} UTC.", clock.UtcNow);
            logger.LogError("This is an error {0} UTC.", clock.UtcNow);

            try
            {
                throw new InvalidOperationException("Oh no, something bad happened.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception happened {0} UTC.", clock.UtcNow);
            }

            telemetryClient.TrackTrace("Explicitly tracked trace.");

            return _next.Invoke(context);
        }
    }
}
