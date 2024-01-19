using Microsoft.ApplicationInsights.Channel;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

/// <summary>
/// Telemetry channel that does nothing so AI can be tested without it sending anything over the Internet. Useful for UI
/// testing, for example.
/// </summary>
internal sealed class NullTelemetryChannel : ITelemetryChannel
{
    public bool? DeveloperMode { get; set; }
    public string EndpointAddress { get; set; }

    public void Dispose() { }
    public void Flush() { }
    public void Send(ITelemetry item) { }
}
