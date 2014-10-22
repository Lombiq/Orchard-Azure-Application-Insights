using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Settings;
using Orchard.ContentManagement;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using log4net;
using log4net.Repository.Hierarchy;
using log4net.Layout;
using Microsoft.ApplicationInsights.Log4NetAppender;
using log4net.Appender;
using Orchard;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
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
            }
        }

        public void Terminating()
        {
        }
    }
}