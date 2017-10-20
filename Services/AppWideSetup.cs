using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.Hosting.Azure.ApplicationInsights.Events;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation.Tracing;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.ApplicationInsights.SnapshotCollector;
using Orchard;
using Orchard.Validation;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    public class AppWideSetup : IAppWideSetup
    {
        private static bool _telemetryModulesInitialized = false;

        private readonly ITelemetryConfigurationFactory _telemetryConfigurationFactory;
        private readonly ITelemetryModulesHolder _telemetryModulesHolder;
        private readonly ITelemetryModulesInitializationEventHandler _telemetryModulesInitializationEventHandler;
        private readonly ILoggerSetup _loggerSetup;
        private readonly IAppWideQuickPulseTelemetryProcessorAccessor _appWideQuickPulseTelemetryProcessorAccessor;


        public AppWideSetup(
            ITelemetryConfigurationFactory telemetryConfigurationFactory,
            ITelemetryModulesHolder telemetryModulesHolder,
            ITelemetryModulesInitializationEventHandler telemetryModulesInitializationEventHandler,
            ILoggerSetup loggerSetup,
            IAppWideQuickPulseTelemetryProcessorAccessor appWideQuickPulseTelemetryProcessorAccessor)
        {
            _telemetryConfigurationFactory = telemetryConfigurationFactory;
            _telemetryModulesHolder = telemetryModulesHolder;
            _telemetryModulesInitializationEventHandler = telemetryModulesInitializationEventHandler;
            _loggerSetup = loggerSetup;
            _appWideQuickPulseTelemetryProcessorAccessor = appWideQuickPulseTelemetryProcessorAccessor;
        }


        public void SetupAppWideServices(IAppWideServicesConfiguration configuration)
        {
            Argument.ThrowIfNull(configuration, nameof(configuration));

            var telemetryConfiguration = configuration.TelemetryConfiguration;
            if (telemetryConfiguration == null) return;

            TelemetryConfiguration.Active.InstrumentationKey = telemetryConfiguration.InstrumentationKey;
            _telemetryConfigurationFactory.PopulateWithCommonConfiguration(TelemetryConfiguration.Active);

            if (configuration.EnableDebugSnapshotCollection)
            {
                telemetryConfiguration.TelemetryProcessorChainBuilder.Use(next => new SnapshotCollectorTelemetryProcessor(next));
                telemetryConfiguration.TelemetryProcessorChainBuilder.Build();

                TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Use(next => new SnapshotCollectorTelemetryProcessor(next));
                TelemetryConfiguration.Active.TelemetryProcessorChainBuilder.Build();
            }

            // Telemetry modules can be only instantiated and initialized once per app domain.
            if (!_telemetryModulesInitialized)
            {
                var telemetryModules = new List<ITelemetryModule>();

                if (configuration.EnableDependencyTracking)
                {
                    telemetryModules.Add(new DependencyTrackingTelemetryModule());
                }

                telemetryModules.Add(new PerformanceCollectorModule());
                telemetryModules.Add(new DiagnosticsTelemetryModule
                {
                    DiagnosticsInstrumentationKey = telemetryConfiguration.InstrumentationKey
                });

                var quickPulseProcessor = _appWideQuickPulseTelemetryProcessorAccessor.GetAppWideQuickPulseTelemetryProcessor();
                var quickPulseModule = new QuickPulseTelemetryModule();
                if (!string.IsNullOrEmpty(configuration.ApiKey)) quickPulseModule.AuthenticationApiKey = configuration.ApiKey;
                quickPulseModule.RegisterTelemetryProcessor(quickPulseProcessor);
                telemetryModules.Add(quickPulseModule);

                _telemetryModulesInitializationEventHandler.TelemetryModulesInitializing(telemetryModules);

                foreach (var telemetryModule in telemetryModules)
                {
                    telemetryModule.Initialize(telemetryConfiguration);
                    _telemetryModulesHolder.RegisterTelemetryModule(telemetryModule);
                }

                _telemetryModulesInitialized = true;
            }

            if (configuration.EnableLogCollection)
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