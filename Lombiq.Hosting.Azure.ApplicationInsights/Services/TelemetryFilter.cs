using Lombiq.Hosting.Azure.ApplicationInsights.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class TelemetryFilter : ITelemetryProcessor
{
    private static readonly List<Regex> _expectedErrors =
    [
        // Using this generic error message to filter because it is not possible to filter only those cases where it is
        // meant to happen to throw this error. Also setting CreateContainer to false won't solve this issue, because
        // Blob Media container is always checked on each tenant activation in OrchardCore
        // MediaBlobContainerTenantEvents class.
        new(@"Azure\.RequestFailedException: The specified container already exists\.", RegexOptions.Compiled, TimeSpan.FromSeconds(1)),
        new(
            @"Microsoft\.Data\.SqlClient\.SqlException \(0x80131904\): There is already an object named '.*_Identifiers' in the database\.",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1)),
    ];

    private readonly ITelemetryProcessor _next;
    private readonly IServiceProvider _serviceProvider;

    public TelemetryFilter(ITelemetryProcessor next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public void Process(ITelemetry item)
    {
        GetDependencyTelemetryFailureToIgnore(item)?.SetAsIgnoredFailure();

        _next.Process(item);
    }

    private DependencyTelemetry GetDependencyTelemetryFailureToIgnore(ITelemetry item)
    {
        var dependency = item as DependencyTelemetry;
        if (dependency is not { Success: false }) return null;

        dependency.Properties.TryGetValue("Error", out var error);
        dependency.Properties.TryGetValue("Exception", out var exception);

        var shouldSetAsIgnoredFailure =
            _expectedErrors.Exists(expectedError => error != null && expectedError.IsMatch(error)) ||
            _expectedErrors.Exists(expectedError => exception != null && expectedError.IsMatch(exception));

        if (shouldSetAsIgnoredFailure)
        {
            return dependency;
        }

        // We are looking for 409 response code, which can indicate a container already exists error.
        if (dependency.ResultCode != "409")
        {
            return null;
        }

        var shellHost = _serviceProvider.GetRequiredService<IShellHost>();
        dependency.Properties.TryGetValue("OrchardCore.ShellName", out var shellName);
        shellHost.TryGetSettings(shellName, out var shellSettings);
        var dataProtectionConnectionString = shellSettings["OrchardCore_DataProtection_Azure:ConnectionString"];

        var dataProtectionContainerName = shellSettings["OrchardCore_DataProtection_Azure:ContainerName"];
        if (string.IsNullOrEmpty(dataProtectionContainerName))
        {
            dataProtectionContainerName = "dataprotection"; // #spell-check-ignore-line
        }

        if (dataProtectionConnectionString.Contains("UseDevelopmentStorage=true"))
        {
            dataProtectionContainerName =
                "/devstoreaccount1/" + dataProtectionContainerName; // #spell-check-ignore-line
        }

        // Name property value could be different depending on the environment, so using the Data property instead.
        if (dependency.Data.Contains(dataProtectionContainerName))
        {
            return dependency;
        }

        // MediaBlobStorageOptions won't be initiated, so directly checking the config.
        var mediaBlobStorageConnectionString = shellSettings["OrchardCore_Media_Azure:ConnectionString"];
        var mediaBlobStorageContainerName = shellSettings["OrchardCore_Media_Azure:ContainerName"];
        if (mediaBlobStorageConnectionString.Contains("UseDevelopmentStorage=true"))
        {
            mediaBlobStorageContainerName =
                "/devstoreaccount1/" + mediaBlobStorageContainerName; // #spell-check-ignore-line
        }

        // Name property value could be different depending on the environment, so using the Data property instead.
        return dependency.Data.Contains(mediaBlobStorageContainerName) ? dependency : null;
    }
}
