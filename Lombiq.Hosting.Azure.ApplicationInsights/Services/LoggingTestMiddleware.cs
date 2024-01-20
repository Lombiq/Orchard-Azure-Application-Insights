using Lombiq.Hosting.Azure.ApplicationInsights.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class LoggingTestMiddleware(RequestDelegate next)
{
    public Task InvokeAsync(
        HttpContext context,
        ILogger<LoggingTestMiddleware> logger,
        IClock clock,
        TelemetryClient telemetryClient)
    {
        if (!context.Request.Query.ContainsKey("logtest"))
        {
            return next.Invoke(context);
        }

        try
        {
            LoggingTestHelper.LogTestMessagesAndThrow(logger, clock, telemetryClient);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception happened {DateTime} UTC.", clock.UtcNow);
        }

        return next.Invoke(context);
    }
}
