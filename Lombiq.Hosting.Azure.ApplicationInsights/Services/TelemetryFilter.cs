﻿using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;
using System.Collections.Generic;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class TelemetryFilter : ITelemetryProcessor
{
    private static readonly List<string> Errors = new()
    {
        // Using this generic error message to filter because it is not possible to filter only those cases where it is
        // meant to happen to throw this error. Also setting CreateContainer to false won't solve this issue, because
        // Blob Media container is always checked on each tenant activation.
        "Azure.RequestFailedException: The specified container already exists.",
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
        var hasError = Errors.Exists(error => dependencyError?.StartsWithOrdinalIgnoreCase(error) == true) ||
            Errors.Exists(error => exception?.StartsWithOrdinalIgnoreCase(error) == true) ||
            exception?.Contains(GetTenantSqlErrorMessage(shellName)) == true;
        var shouldSend = !hasError;

        if (!shouldSend)
        {
            return false;
        }

        dependency.Properties.TryGetValue("OrchardCore.DataProtectionContainerName", out var dataProtectionContainerName);
        dependency.Properties.TryGetValue("OrchardCore.MediaBlobStorageContainerName", out var mediaBlobStorageContainerName);
        shouldSend = !(dependency.Name == "PUT " + dataProtectionContainerName ||
            dependency.Name == "PUT " + mediaBlobStorageContainerName);

        return shouldSend;
    }

    private static string GetTenantSqlErrorMessage(string tenantName) =>
        $"Microsoft.Data.SqlClient.SqlException (0x80131904): There is already an object named '{tenantName}_Identifiers' in the database.";
}
