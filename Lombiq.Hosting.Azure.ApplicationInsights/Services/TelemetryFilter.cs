using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class TelemetryFilter : ITelemetryProcessor
{
    private static readonly List<string> Errors = new()
    {
        "Azure.RequestFailedException: The specified container already exists.",
        "The specified blob does not exist.",
        "Microsoft.Data.SqlClient.SqlException (0x80131904): There is already an object named 'Shells_Identifiers' in the database.",
    };

    private readonly ITelemetryProcessor _next;

    public TelemetryFilter(ITelemetryProcessor next) => _next = next;

    public void Process(ITelemetry item)
    {
        if (!ShouldSend(item)) { return; }

        _next.Process(item);
    }

    private static bool ShouldSend(ITelemetry item)
    {
        var dependency = item as DependencyTelemetry;
        if (dependency is not { Success: false }) return true;

        dependency.Properties.TryGetValue("Error", out var dependencyError);
        dependency.Properties.TryGetValue("Exception", out var exception);
        dependency.Properties.TryGetValue("OrchardCore.ShellName", out var shellName);
        var shouldSend = !(Errors.Exists(error => dependencyError?.StartsWith(error, StringComparison.OrdinalIgnoreCase) == true) ||
            Errors.Exists(error => exception?.StartsWith(error, StringComparison.OrdinalIgnoreCase) == true) ||
            exception?.Contains(GetTenantSqlErrorMessage(shellName)) == true);

        return shouldSend;
    }

    private static string GetTenantSqlErrorMessage(string tenantName) =>
        $"Microsoft.Data.SqlClient.SqlException (0x80131904): There is already an object named '{tenantName}_Identifiers' in the database.";
}
