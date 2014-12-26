using Orchard;
using Orchard.Environment;
using Orchard.Environment.Configuration;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Sets up AI logging on shell start.
    /// </summary>
    public class LoggerSetupShellEventHandler : IOrchardShellEvents
    {
        private readonly ShellSettings _shellSettings;
        private readonly IWorkContextAccessor _wca;


        public LoggerSetupShellEventHandler(
            ShellSettings shellSettings,
            IWorkContextAccessor wca)
        {
            _shellSettings = shellSettings;
            _wca = wca;
        }
        
        
        public void Activated()
        {
            // Log4Net configuration is application-wide, thus should happen only once.
            if (_shellSettings.Name != ShellSettings.DefaultName) return;

            // ISiteService couldn't be resolved because there is no work context during shell startup, that's
            // the reason for the custom work context.
            using (var wc = _wca.CreateWorkContextScope())
            {
                var settings = wc.Resolve<ITelemetrySettingsAccessor>().GetDefaultSettings();

                if (!settings.ApplicationWideLogCollectionIsEnabled || string.IsNullOrEmpty(settings.InstrumentationKey)) return;

                wc.Resolve<ILoggerSetup>().SetupAiAppender(Constants.DefaultLogAppenderName, settings.InstrumentationKey);
                wc.Resolve<IStartupLogEntriesCollector>().ReLogStartupLogEntriesIfNew();
            }
        }

        public void Terminating()
        {
        }
    }
}