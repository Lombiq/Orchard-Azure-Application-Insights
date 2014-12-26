
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
        /// Indicates whether log entries are collected and sent to AI application-wide.
        /// </summary>
        bool ApplicationWideLogCollectionIsEnabled { get; }
    }
}
