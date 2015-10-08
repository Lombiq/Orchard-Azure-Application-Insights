using Microsoft.ApplicationInsights.Extensibility;
using Orchard.Events;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Events
{
    /// <summary>
    /// Exposes events for the life cycle of <see cref="TelemetryConfiguration"/> objects.
    /// </summary>
    public interface ITelemetryConfigurationEventHandler : IEventHandler
    {
        /// <summary>
        /// Fires when the <see cref="TelemetryConfiguration"/> is fully populated with basic configuration. You can alter 
        /// the configuration through this event.
        /// </summary>
        /// <param name="telemetryConfiguration">The configuration currently loaded.</param>
        void ConfigurationLoaded(TelemetryConfiguration telemetryConfiguration);
    }
}
