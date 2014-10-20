using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
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
        private readonly IDefaultTelemetryConfigurationAccessor _defaultTelemetryConfigurationAccessor;


        public TelemetryClientFactory(IDefaultTelemetryConfigurationAccessor defaultTelemetryConfigurationAccessor)
        {
            _defaultTelemetryConfigurationAccessor = defaultTelemetryConfigurationAccessor;
        }
        
        
        public TelemetryClient CreateTelemetryClient(TelemetryConfiguration configuration)
        {
            return new TelemetryClient(configuration);
        }

        public TelemetryClient CreateTelemetryClientFromDefaultConfiguration()
        {
            return CreateTelemetryClient(_defaultTelemetryConfigurationAccessor.GetDefaultConfiguration());
        }
    }
}