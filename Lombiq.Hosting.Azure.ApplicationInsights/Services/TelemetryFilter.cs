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
        if (!ShouldSend(item)) { return; }

        _next.Process(item);
    }

    private bool ShouldSend(ITelemetry item)
    {
        var dependency = item as DependencyTelemetry;
        if (dependency is not { Success: false }) return true;

        dependency.Properties.TryGetValue("Error", out var error);
        dependency.Properties.TryGetValue("Exception", out var exception);
        return !_errors.Contains(error) && !_errors.Contains(exception);
    }
}
