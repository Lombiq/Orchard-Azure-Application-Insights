using Lombiq.Hosting.Azure.ApplicationInsights.Events;
using Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;
using Orchard;
using Orchard.Environment.Configuration;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Builds <see cref="TelemetryConfiguration"/> objects. If you want to cut down on objects, try to limit the number of
    /// <see cref="TelemetryConfiguration"/> objects rather than e.g. <see cref="Microsoft.ApplicationInsights.TelemetryClient"/>
    /// objects.
    /// </summary>
    public interface ITelemetryConfigurationFactory : IDependency
    {
        /// <summary>
        /// Creates a <see cref="TelemetryConfiguration"/> object with the common telemetry modules and context initializers,
        /// using the default channel and the given instrumentation key.
        /// </summary>
        /// <param name="instrumentationKey">The instrumentation key to access telemetry services on Azure.</param>
        TelemetryConfiguration CreateConfiguration(string instrumentationKey);

        /// <summary>
        /// Populates an existing <see cref="TelemetryConfiguration"/> object with e.g. common telemetry modules and 
        /// context initializers.
        /// </summary>
        /// <param name="configuration">The existing configuration object.</param>
        void PopulateWithCommonConfiguration(TelemetryConfiguration configuration);
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
            TelemetryConfiguration.CreateDefault();

            var configuration = TelemetryConfiguration.CreateDefault();
            configuration.InstrumentationKey = instrumentationKey;

            PopulateWithCommonConfiguration(configuration);

            return configuration;
        }

        public void PopulateWithCommonConfiguration(TelemetryConfiguration configuration)
        {
            var telemetryInitializers = configuration.TelemetryInitializers;
            telemetryInitializers.Add(new WebOperationIdTelemetryInitializer());
            telemetryInitializers.Add(new ShellNameTelemetryInitializer());


            _telemetryConfigurationEventHandler.ConfigurationLoaded(configuration);
        }
    }
}