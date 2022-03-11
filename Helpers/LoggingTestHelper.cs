using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Logging;
using OrchardCore.Modules;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Helpers
{
    internal static class LoggingTestHelper
    {
        public static void LogTestMessagesAndThrow(
            ILogger logger,
            IClock clock,
            TelemetryClient telemetryClient)
        {
            logger.LogTrace("This is a trace at {DateTime} UTC.", clock.UtcNow);
            logger.LogDebug("This is a debug message {DateTime} UTC.", clock.UtcNow);
            logger.LogInformation("This is an info message {DateTime} UTC.", clock.UtcNow);
            logger.LogWarning("This is a warning {DateTime} UTC.", clock.UtcNow);
            logger.LogError("This is an error {DateTime} UTC.", clock.UtcNow);

            telemetryClient.TrackTrace("Explicitly tracked trace.");

            throw new InvalidOperationException("Oh no, something bad happened.");
        }
    }
}
