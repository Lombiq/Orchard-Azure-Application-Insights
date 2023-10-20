using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class TelemetryFilter : ITelemetryProcessor
{
    private static readonly List<Regex> _expectedErrors = new()
    {
        // Using this generic error message to filter because it is not possible to filter only those cases where it is
        // meant to happen to throw this error. Also setting CreateContainer to false won't solve this issue, because
        // Blob Media container is always checked on each tenant activation in OrchardCore
        // MediaBlobContainerTenantEvents class.
        new(@"Azure\.RequestFailedException: The specified container already exists\.", RegexOptions.None, TimeSpan.FromSeconds(1)),
        new(
            @"Microsoft\.Data\.SqlClient\.SqlException \(0x80131904\): There is already an object named '.*_Identifiers' in the database\.",
            RegexOptions.None,
            TimeSpan.FromSeconds(1)),
    };

    private readonly ITelemetryProcessor _next;
    private readonly IServiceProvider _serviceProvider;

    public TelemetryFilter(ITelemetryProcessor next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public void Process(ITelemetry item)
    {
        if (ShouldNotSend(item)) { return; }

        _next.Process(item);
    }

    private bool ShouldNotSend(ITelemetry item)
    {
        var dependency = item as DependencyTelemetry;
        if (dependency is not { Success: false }) return false;

        dependency.Properties.TryGetValue("Error", out var error);
        dependency.Properties.TryGetValue("Exception", out var exception);

        var shouldNotSend = _expectedErrors.Exists(expectedError => error != null && expectedError.IsMatch(error)) ||
            _expectedErrors.Exists(expectedError => exception != null && expectedError.IsMatch(exception));

        if (shouldNotSend)
        {
            return true;
        }

        // We are looking for 409 response code, which can indicate a container already exists error.
        if (dependency.ResultCode != "409")
        {
            return false;
        }

        var shellConfiguration = _serviceProvider.GetRequiredService<IShellConfiguration>();
        var dataProtectionConnectionString = shellConfiguration
            .GetValue<string>("OrchardCore_DataProtection_Azure:ConnectionString");

        var dataProtectionContainerName = shellConfiguration
            .GetValue("OrchardCore_DataProtection_Azure:ContainerName", "dataprotection"); // #spell-check-ignore-line

        if (dataProtectionConnectionString.Contains("UseDevelopmentStorage=true"))
        {
            dataProtectionContainerName = "/devstoreaccount1/" + dataProtectionContainerName; // #spell-check-ignore-line
        }

        if (dependency.Name == "PUT " + dataProtectionContainerName)
        {
            return true;
        }

        // MediaBlobStorageOptions won't be initiated, so directly checking the config.
        var mediaBlobStorageConnectionString = shellConfiguration
            .GetValue<string>("OrchardCore_Media_Azure:ConnectionString");
        var mediaBlobStorageContainerName = shellConfiguration
            .GetValue<string>("OrchardCore_Media_Azure:ContainerName");
        if (mediaBlobStorageConnectionString.Contains("UseDevelopmentStorage=true"))
        {
            mediaBlobStorageContainerName = "/devstoreaccount1/" + mediaBlobStorageContainerName; // #spell-check-ignore-line
        }

        return dependency.Name == "PUT " + mediaBlobStorageContainerName;
    }
}
