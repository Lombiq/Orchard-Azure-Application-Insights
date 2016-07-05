using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Builds <see cref="TelemetryClient"/> objects.
    /// </summary>
    public interface ITelemetryClientFactory : IDependency
    {
        /// <summary>
        /// Creates a <see cref="TelemetryClient"/> object with the given configuration.
        /// </summary>
        /// <param name="configuration">The <see cref="TelemetryConfiguration"/> to use when constructing the object.</param>
        TelemetryClient CreateTelemetryClient(TelemetryConfiguration configuration);

        /// <summary>
        /// Creates a <see cref="TelemetryClient"/> object with the current configuration that can be modified from the 
        /// admin or set from static configuration.
        /// </summary>
        TelemetryClient CreateTelemetryClientFromCurrentConfiguration();
    }


    public class TelemetryClientFactory : ITelemetryClientFactory
    {
        private readonly ITelemetryConfigurationAccessor _telemetryConfigurationAccessor;


        public TelemetryClientFactory(ITelemetryConfigurationAccessor telemetryConfigurationAccessor)
        {
            _telemetryConfigurationAccessor = telemetryConfigurationAccessor;
        }
        
        
        public TelemetryClient CreateTelemetryClient(TelemetryConfiguration configuration)
        {
            return new TelemetryClient(configuration);
        }

        public TelemetryClient CreateTelemetryClientFromCurrentConfiguration()
        {
            var defaultConfiguration = _telemetryConfigurationAccessor.GetCurrentConfiguration();
            if (defaultConfiguration == null) return null;
            return CreateTelemetryClient(defaultConfiguration);
        }
    }
}