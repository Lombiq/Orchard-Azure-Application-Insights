using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Orchard;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using System.Linq;
using Lombiq.Hosting.Azure.ApplicationInsights.Events;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Sets up global AI configuration on shell start.
    /// </summary>
    /// <remarks>
    /// That there is such global setup is unfortunate, however with the current design of AI there is simply a need for
    /// an "Active" configuration.
    /// Also logging can currently only happen application-wide, since from the logger it's not always possible to
    /// determine the current tenant (and when logging application-level entries, there is no tenant).
    /// </remarks>
    public class GlobalSetupShellEventHandler : IOrchardShellEvents
    {
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _wca;


        public GlobalSetupShellEventHandler(
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

            // ISiteService couldn't be resolved because there is no work context during shell startup, that's
            // the reason for the custom work context.
            // See: https://github.com/OrchardCMS/Orchard/issues/4852
            using (var wc = _wca.CreateWorkContextScope())
            {
                var settings = wc.Resolve<ITelemetrySettingsAccessor>().GetCurrentSettings();

                if (string.IsNullOrEmpty(settings.InstrumentationKey)) return;

                TelemetryConfiguration.Active.InstrumentationKey = settings.InstrumentationKey;
                wc.Resolve<ITelemetryConfigurationFactory>().PopulateWithCommonConfiguration(TelemetryConfiguration.Active);

                var telemetryModulesHolder = wc.Resolve<ITelemetryModulesHolder>();
                telemetryModulesHolder.RegisterTelemetryModule(new DependencyTrackingTelemetryModule());
                telemetryModulesHolder.RegisterTelemetryModule(new PerformanceCollectorModule());
                var registeredTelemetryModules = telemetryModulesHolder.GetRegisteredModules().ToList();
                wc.Resolve<ITelemetryModulesInitializationEventHandler>().TelemetryModulesInitializing(registeredTelemetryModules);
                foreach (var telemetryModule in registeredTelemetryModules)
                {
                    telemetryModule.Initialize(TelemetryConfiguration.Active);
                }

                if (settings.ApplicationWideLogCollectionIsEnabled)
                {
                    wc.Resolve<ILoggerSetup>().SetupAiAppender(Constants.DefaultLogAppenderName, settings.InstrumentationKey);
                    wc.Resolve<IStartupLogEntriesCollector>().ReLogStartupLogEntriesIfNew();
                }
            }
        }

        public void Terminating()
        {
        }
    }
}