using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using System.Linq;
using Lombiq.Hosting.Azure.ApplicationInsights.Events;
using Orchard.Settings;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Orchard.ContentManagement;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Sets up application-wide AI configuration on shell start.
    /// </summary>
    /// <remarks>
    /// That there is such global setup is unfortunate, however with the current design of AI there is simply a need for
    /// an "Active" configuration.
    /// Also logging can currently only happen application-wide, since from the logger it's not always possible to
    /// determine the current tenant (and when logging application-level entries, there is no tenant).
    /// </remarks>
    public class AppWideSetupShellEventHandler : IOrchardShellEvents
    {
        private readonly ShellSettings _shellSettings;
        private readonly ITelemetryConfigurationAccessor _telemetryConfigurationAccessor;
        private readonly ISiteService _siteService;
        private readonly IAppWideSetup _appWideSetup;
        private readonly IPreviousLogEntriesCollector _previousLogEntriesCollector;


        public AppWideSetupShellEventHandler(
            ShellSettings shellSettings,
            ITelemetryConfigurationAccessor telemetryConfigurationAccessor,
            ISiteService siteService,
            IAppWideSetup appWideSetup,
            IPreviousLogEntriesCollector previousLogEntriesCollector)
        {
            _shellSettings = shellSettings;
            _telemetryConfigurationAccessor = telemetryConfigurationAccessor;
            _siteService = siteService;
            _appWideSetup = appWideSetup;
            _previousLogEntriesCollector = previousLogEntriesCollector;
        }


        public void Activated()
        {
            // Global configuration is application-wide, thus should happen only once.
            if (_shellSettings.Name != ShellSettings.DefaultName) return;

            var currentConfiguration = _telemetryConfigurationAccessor.GetCurrentConfiguration();

            if (currentConfiguration == null) return;

            var settingsPart = _siteService.GetSiteSettings().As<AzureApplicationInsightsTelemetrySettingsPart>();

            _appWideSetup.SetupAppWideServices(
                currentConfiguration,
                settingsPart.ApplicationWideDependencyTrackingIsEnabled,
                settingsPart.ApplicationWideLogCollectionIsEnabled);

            if (settingsPart.ApplicationWideLogCollectionIsEnabled)
            {
                _previousLogEntriesCollector.ReLogPreviousLogEntries();
            }
        }


        public void Terminating()
        {
        }
    }
}