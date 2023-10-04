using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System.Collections.Generic;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class TelemetryFilter : ITelemetryProcessor
{
    private readonly List<string> _errors = new()
    {
        "Azure.RequestFailedException: The specified container already exists.",
        "The specified blob does not exist.",
        "Microsoft.Data.SqlClient.SqlException (0x80131904): There is already an object named",
    };
    private readonly ITelemetryProcessor _next;

    public TelemetryFilter(ITelemetryProcessor next) =>
        _next = next;

    public void Process(ITelemetry item)
    {
        if (!OKtoSend(item)) { return; }

        _next.Process(item);
    }

    private bool OKtoSend(ITelemetry item)
    {
        var dependency = item as DependencyTelemetry;
        if (dependency is not { Success: false }) return true;

        return !_errors.Contains(dependency.Properties["Error"]) && !_errors.Contains(dependency.Properties["Exception"]);
    }
}
