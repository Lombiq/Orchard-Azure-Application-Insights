using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights
{
    public class TestMiddleware
    {
        private readonly RequestDelegate _next;

        public TestMiddleware(RequestDelegate next) => _next = next;

        public Task InvokeAsync(HttpContext context, ILogger<TestMiddleware> logger, TelemetryClient telemetryClient, IServiceProvider serviceProvider)
        {
            logger.LogTrace("this is trace");
            logger.LogDebug("this is debug");
            logger.LogInformation("this is info");
            logger.LogWarning("this is warning");
            logger.LogError("this is error");

            try
            {
                throw new System.Exception("oh no");
            }
            catch (System.Exception ex)
            {
                logger.LogError(ex, "exception: ");
            }

            //telemetryClient.TrackTrace("tracked trace");

            return _next.Invoke(context);
        }
    }
}
