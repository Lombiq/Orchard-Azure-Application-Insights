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
            logger.LogTrace("This is a trace at {0} UTC.", clock.UtcNow);
            logger.LogDebug("This is a debug message {0} UTC.", clock.UtcNow);
            logger.LogInformation("This is an info message {0} UTC.", clock.UtcNow);
            logger.LogWarning("This is a warning {0} UTC.", clock.UtcNow);
            logger.LogError("This is an error {0} UTC.", clock.UtcNow);

            telemetryClient.TrackTrace("Explicitly tracked trace.");

            throw new InvalidOperationException("Oh no, something bad happened.");
        }
    }
}
