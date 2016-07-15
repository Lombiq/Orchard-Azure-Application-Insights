using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights.DataContracts;
using Orchard.Environment.Configuration;
using Orchard.Services;
using Orchard.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services.BackgroundTaskTelemetry
{
    /// <summary>
    /// Decorator for the default <see cref="IBackgroundService"/> implementation so background sweeps can be tracked
    /// as an individual operation.
    /// </summary>
    internal class BackgroundServiceDecorator : IBackgroundService
    {
        private readonly IBackgroundService _decorated;
        private readonly ITelemetryClientFactory _telemetryClientFactory;
        private readonly ShellSettings _shellSettings;
        private readonly IEnumerable<IBackgroundTask> _backgroundTasks;
        private readonly IClock _clock;


        public BackgroundServiceDecorator(
            IBackgroundService decorated,
            ITelemetryClientFactory telemetryClientFactory,
            ShellSettings shellSettings,
            IEnumerable<IBackgroundTask> backgroundTasks,
            IClock clock)
        {
            _decorated = decorated;
            _telemetryClientFactory = telemetryClientFactory;
            _shellSettings = shellSettings;
            _backgroundTasks = backgroundTasks;
            _clock = clock;
        }


        public void Sweep()
        {
            var beginTime = _clock.UtcNow;

            _decorated.Sweep();

            var telemetryClient = _telemetryClientFactory.CreateTelemetryClientFromCurrentConfiguration();

            var eventTelemetry = new EventTelemetry { Name = "BackgroundServiceSweepEnd", Timestamp = _clock.UtcNow };
            eventTelemetry.SetShellName(_shellSettings);
            eventTelemetry.Metrics["Orchard.BackgroundTasksExecutionTimeSeconds"] = (_clock.UtcNow - beginTime).Seconds;
            eventTelemetry.Metrics["Orchard.BackgroundTaskCount"] = _backgroundTasks.Count();
            eventTelemetry.Properties["Orchard.BackgroundTasks"] =
                string.Join(", ", _backgroundTasks.Select(task => task.GetType().FullName));

            telemetryClient.TrackEvent(eventTelemetry);
        }
    }
}