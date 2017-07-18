
namespace Lombiq.Hosting.Azure.ApplicationInsights.Models
{
    /// <summary>
    /// Describes the basic settings for AI telemetry usage.
    /// </summary>
    public interface ITelemetrySettings
    {
        /// <summary>
        /// The instrumentation key to access telemetry services on Azure.
        /// </summary>
        string InstrumentationKey { get; }

        /// <summary>
        /// The API key used to set up a secure control channel with Azure. This is used in Live Metrics Stream filtering
        /// for example.
        /// </summary>
        string ApiKey { get; }
    }
}
