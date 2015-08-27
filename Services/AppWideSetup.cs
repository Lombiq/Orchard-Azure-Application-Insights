using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.Hosting.Azure.ApplicationInsights.Events;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Orchard;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    public interface IAppWideSetup : IDependency
    {
        void SetupAppWideServices(TelemetryConfiguration telemetryConfiguration, bool enableDependencyTracking, bool enableLogCollection);
    }


    public class AppWideSetup : IAppWideSetup
    {
        private readonly ITelemetryConfigurationFactory _telemetryConfigurationFactory;
        private readonly ITelemetryModulesHolder _telemetryModulesHolder;
        private readonly ITelemetryModulesInitializationEventHandler _telemetryModulesInitializationEventHandler;
        private readonly ILoggerSetup _loggerSetup;


        public AppWideSetup(
            ITelemetryConfigurationFactory telemetryConfigurationFactory,
            ITelemetryModulesHolder telemetryModulesHolder,
            ITelemetryModulesInitializationEventHandler telemetryModulesInitializationEventHandler,
            ILoggerSetup loggerSetup)
        {
            _telemetryConfigurationFactory = telemetryConfigurationFactory;
            _telemetryModulesHolder = telemetryModulesHolder;
            _telemetryModulesInitializationEventHandler = telemetryModulesInitializationEventHandler;
            _loggerSetup = loggerSetup;
        }


        public void SetupAppWideServices(TelemetryConfiguration telemetryConfiguration, bool enableDependencyTracking, bool enableLogCollection)
        {
            if (telemetryConfiguration == null) return;

            TelemetryConfiguration.Active.InstrumentationKey = telemetryConfiguration.InstrumentationKey;
            _telemetryConfigurationFactory.PopulateWithCommonConfiguration(TelemetryConfiguration.Active);

            // Can't re-initialize modules with a changed configuration, so need to re-register all of them.
            _telemetryModulesHolder.Clear();

            var telemetryModules = new List<ITelemetryModule>();
            if (enableDependencyTracking)
            {
                telemetryModules.Add(new DependencyTrackingTelemetryModule());
            }
            telemetryModules.Add(new PerformanceCollectorModule());
            _telemetryModulesInitializationEventHandler.TelemetryModulesInitializing(telemetryModules);
            foreach (var telemetryModule in telemetryModules)
            {
                telemetryModule.Initialize(telemetryConfiguration);
                _telemetryModulesHolder.RegisterTelemetryModule(telemetryModule);
            }

            if (enableLogCollection)
            {
                _loggerSetup.SetupAiAppender(Constants.DefaultLogAppenderName, telemetryConfiguration.InstrumentationKey);
            }
            else
            {
                _loggerSetup.RemoveAiAppender(Constants.DefaultLogAppenderName);
            }
        }
    }
}