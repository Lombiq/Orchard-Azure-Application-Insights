using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Microsoft.ApplicationInsights.DataContracts;
using Orchard.ContentManagement;
using Orchard.Environment.Configuration;
using Orchard.Services;
using Orchard.Settings;
using Orchard.Tasks;
using System.Collections.Generic;
using System.Linq;

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
        private readonly ISiteService _siteService;


        public BackgroundServiceDecorator(
            IBackgroundService decorated,
            ITelemetryClientFactory telemetryClientFactory,
            ShellSettings shellSettings,
            IEnumerable<IBackgroundTask> backgroundTasks,
            IClock clock,
            ISiteService siteService)
        {
            _decorated = decorated;
            _telemetryClientFactory = telemetryClientFactory;
            _shellSettings = shellSettings;
            _backgroundTasks = backgroundTasks;
            _clock = clock;
            _siteService = siteService;
        }


        public void Sweep()
        {
            var settingsPart = _siteService.GetSiteSettings().As<AzureApplicationInsightsTelemetrySettingsPart>();

            if (!settingsPart.BackgroundTaskTrackingIsEnabled) return;

            var beginTime = _clock.UtcNow;

            _decorated.Sweep();

            var telemetryClient = _telemetryClientFactory.CreateTelemetryClientFromCurrentConfiguration();

            if (telemetryClient == null) return;

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