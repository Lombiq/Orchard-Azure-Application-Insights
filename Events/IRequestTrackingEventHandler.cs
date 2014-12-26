using Microsoft.ApplicationInsights.DataContracts;
using Orchard.Events;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Events
{
    /// <summary>
    /// Exposes events for HTTP request tracking.
    /// </summary>
    public interface IRequestTrackingEventHandler : IEventHandler
    {
        /// <summary>
        /// Fires when the HTTP request begins.
        /// </summary>
        /// <param name="requestTelemetry">The telemetry object for the request, containing pre-filled data.</param>
        void OnBeginRequest(RequestTelemetry requestTelemetry);

        /// <summary>
        /// Fires when the HTTP request ends.
        /// </summary>
        /// <param name="requestTelemetry">The telemetry object for the request, containing pre-filled data.</param>
        void OnEndRequest(RequestTelemetry requestTelemetry);
    }
}
