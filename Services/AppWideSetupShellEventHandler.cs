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
        private readonly IWorkContextAccessor _wca;


        public AppWideSetupShellEventHandler(
            ShellSettings shellSettings,
            IWorkContextAccessor wca)
        {
            _shellSettings = shellSettings;
            _wca = wca;
        }


        public void Activated()
        {
            // Global configuration is application-wide, thus should happen only once.
            if (_shellSettings.Name != ShellSettings.DefaultName) return;

            // ISiteService can't be resolved because there is no work context during shell startup, that's
            // the reason for the custom work context.
            // See: https://github.com/OrchardCMS/Orchard/issues/4852
            using (var wc = _wca.CreateWorkContextScope())
            {
                var currentConfiguration = wc.Resolve<ITelemetryConfigurationAccessor>().GetCurrentConfiguration();

                if (currentConfiguration == null) return;

                var settingsPart = wc.Resolve<ISiteService>().GetSiteSettings().As<AzureApplicationInsightsTelemetrySettingsPart>();

                wc.Resolve<IAppWideSetup>().SetupAppWideServices(
                    currentConfiguration,
                    settingsPart.ApplicationWideDependencyTrackingIsEnabled,
                    settingsPart.ApplicationWideLogCollectionIsEnabled);

                if (settingsPart.ApplicationWideLogCollectionIsEnabled)
                {
                    wc.Resolve<IPreviousLogEntriesCollector>().ReLogPreviousLogEntries();
                }
            }
        }

        public void Terminating()
        {
        }
    }
}