using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class TelemetryFilter : ITelemetryProcessor
{
    private ITelemetryProcessor _next;
    public TelemetryFilter(ITelemetryProcessor next)
    {
        _next = next;
    }

    public void Process(ITelemetry item)
    {
        // To filter out an item, return without calling the next processor.
        if (!OKtoSend(item)) { return; }

        _next.Process(item);
    }

    private bool OKtoSend (ITelemetry item)
    {
        var dependency = item as DependencyTelemetry;
        if (dependency is not { Success: false }) return true;

        if (dependency.Name == "BlobBaseClient.Exists" ||
            dependency.Name == "Blob.GetProperties" ||
            (dependency.Type == "SQL" && dependency.ResultCode == "2714"))
        {
            var asd = "asd";
        }
        return true;
    }
}
