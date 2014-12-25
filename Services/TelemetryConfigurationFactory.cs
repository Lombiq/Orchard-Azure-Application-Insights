using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.Hosting.Azure.ApplicationInsights.Events;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.RuntimeTelemetry;
using Orchard;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Builds <see cref="TelemetryConfiguration"/> objects.
    /// </summary>
    public interface ITelemetryConfigurationFactory : IDependency
    {
        /// <summary>
        /// Creates a <see cref="TelemetryConfiguration"/> object with the common telemetry modules and context initializers,
        /// using the default channel and the given instrumentation key.
        /// </summary>
        /// <param name="instrumentationKey">The instrumentation key to access telemetry services on Azure.</param>
        TelemetryConfiguration CreateConfiguration(string instrumentationKey);
    }


    public class TelemetryConfigurationFactory : ITelemetryConfigurationFactory
    {
        private readonly ITelemetryConfigurationEventHandler _telemetryConfigurationEventHandler;


        public TelemetryConfigurationFactory(ITelemetryConfigurationEventHandler telemetryConfigurationEventHandler)
        {
            _telemetryConfigurationEventHandler = telemetryConfigurationEventHandler;
        }
        
        

        public TelemetryConfiguration CreateConfiguration(string instrumentationKey)
        {
            var configuration = new TelemetryConfiguration
            {
                InstrumentationKey = instrumentationKey
            };

            // DiagnosticsTelemetryModule is internal and can't be added but it's not needed since it only helps debugging.
            var telemetryModules = configuration.TelemetryModules;
            telemetryModules.Add(new RemoteDependencyModule());

            var contextInitializers = configuration.ContextInitializers;
            contextInitializers.Add(new ComponentContextInitializer());
            contextInitializers.Add(new DeviceContextInitializer());

            _telemetryConfigurationEventHandler.ConfigurationLoaded(configuration);

            return configuration;
        }
    }
}