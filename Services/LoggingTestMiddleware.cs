using Lombiq.Hosting.Azure.ApplicationInsights.Helpers;
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
            IClock clock,
            TelemetryClient telemetryClient)
        {
            if (!context.Request.Query.ContainsKey("logtest"))
            {
                return _next.Invoke(context);
            }

            try
            {
                LoggingTestHelper.LogTestMessagesAndThrow(logger, clock, telemetryClient);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An exception happened {DateTime} UTC.", clock.UtcNow);
            }

            return _next.Invoke(context);
        }
    }
}
