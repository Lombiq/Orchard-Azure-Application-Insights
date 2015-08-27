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
        /// Creates a <see cref="TelemetryClient"/> object with the default configuration that can be modified from the admin.
        /// </summary>
        TelemetryClient CreateTelemetryClientFromDefaultConfiguration();
    }


    public class TelemetryClientFactory : ITelemetryClientFactory
    {
        private readonly ITelemetryConfigurationAccessor _defaultTelemetryConfigurationAccessor;


        public TelemetryClientFactory(ITelemetryConfigurationAccessor defaultTelemetryConfigurationAccessor)
        {
            _defaultTelemetryConfigurationAccessor = defaultTelemetryConfigurationAccessor;
        }
        
        
        public TelemetryClient CreateTelemetryClient(TelemetryConfiguration configuration)
        {
            return new TelemetryClient(configuration);
        }

        public TelemetryClient CreateTelemetryClientFromDefaultConfiguration()
        {
            var defaultConfiguration = _defaultTelemetryConfigurationAccessor.GetCurrentConfiguration();
            if (defaultConfiguration == null) return null;
            return CreateTelemetryClient(defaultConfiguration);
        }
    }
}