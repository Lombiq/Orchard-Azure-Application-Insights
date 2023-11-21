using Lombiq.Hosting.Azure.ApplicationInsights.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;

public class IgnoreFailureInitializer : ITelemetryInitializer
{
    private static readonly List<Regex> _expectedErrors = new()
    {
        // Using this generic error message to filter because it is not possible to filter only those cases where it is
        // meant to happen to throw this error. Also setting CreateContainer to false won't solve this issue, because
        // Blob Media container is always checked on each tenant activation in OrchardCore
        // MediaBlobContainerTenantEvents class.
        new(
            @"Azure\.RequestFailedException: The specified container already exists\.",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1)),
        new(
            @"Microsoft\.Data\.SqlClient\.SqlException \(0x80131904\): There is already an object named '.*_Identifiers' in the database\.",
            RegexOptions.Compiled,
            TimeSpan.FromSeconds(1)),
    };

    private readonly IServiceProvider _serviceProvider;

    public IgnoreFailureInitializer(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public void Initialize(ITelemetry telemetry)
    {
        var operationTelemetry = telemetry as OperationTelemetry;
        if (operationTelemetry?.Success != false) return;

        var options = _serviceProvider.GetRequiredService<IOptions<ApplicationInsightsOptions>>().Value;

        if (operationTelemetry is RequestTelemetry requestTelemetry &&
            int.Parse(requestTelemetry.ResponseCode, CultureInfo.InvariantCulture) is >= 400 and < 500 &&
            options.IgnoreFailureRegex.IsMatch(requestTelemetry.Url.ToString()))
        {
            requestTelemetry.SetAsIgnoredFailure();
            return;
        }

        if (operationTelemetry is not DependencyTelemetry dependencyTelemetry)
        {
            return;
        }

        if (!string.IsNullOrEmpty(dependencyTelemetry.ResultCode) &&
            int.Parse(dependencyTelemetry.ResultCode, CultureInfo.InvariantCulture) is >= 400 and < 500 &&
            options.IgnoreFailureRegex.IsMatch(dependencyTelemetry.Data))
        {
            dependencyTelemetry.SetAsIgnoredFailure();
            return;
        }

        if (ShouldSetAsIgnoredFailure(dependencyTelemetry))
        {
            dependencyTelemetry.SetAsIgnoredFailure();
        }
    }

    private bool ShouldSetAsIgnoredFailure(DependencyTelemetry dependencyTelemetry)
    {
        dependencyTelemetry.Properties.TryGetValue("Error", out var error);
        dependencyTelemetry.Properties.TryGetValue("Exception", out var exception);

        var shouldSetAsIgnoredFailure =
            _expectedErrors.Exists(expectedError => error != null && expectedError.IsMatch(error)) ||
            _expectedErrors.Exists(expectedError => exception != null && expectedError.IsMatch(exception));

        if (shouldSetAsIgnoredFailure)
        {
            return true;
        }

        // We are looking for 409 response code, which can indicate a container already exists error.
        if (dependencyTelemetry.ResultCode != "409")
        {
            return false;
        }

        var shellHost = _serviceProvider.GetRequiredService<IShellHost>();
        dependencyTelemetry.Properties.TryGetValue("OrchardCore.ShellName", out var shellName);
        shellHost.TryGetSettings(shellName, out var shellSettings);
        var dataProtectionConnectionString = shellSettings["OrchardCore_DataProtection_Azure:ConnectionString"];

        var dataProtectionContainerName = shellSettings["OrchardCore_DataProtection_Azure:ContainerName"];
        if (string.IsNullOrEmpty(dataProtectionContainerName))
        {
            dataProtectionContainerName = "dataprotection";// #spell-check-ignore-line
        }

        if (dataProtectionConnectionString.Contains("UseDevelopmentStorage=true"))
        {
            dataProtectionContainerName =
                "/devstoreaccount1/" + dataProtectionContainerName;// #spell-check-ignore-line
        }

        // Name property value could be different depending on the environment, so using the Data property instead.
        if (dependencyTelemetry.Data.Contains(dataProtectionContainerName))
        {
            return true;
        }

        // MediaBlobStorageOptions won't be initiated, so directly checking the config.
        var mediaBlobStorageConnectionString = shellSettings["OrchardCore_Media_Azure:ConnectionString"];
        var mediaBlobStorageContainerName = shellSettings["OrchardCore_Media_Azure:ContainerName"];
        if (mediaBlobStorageConnectionString.Contains("UseDevelopmentStorage=true"))
        {
            mediaBlobStorageContainerName =
                "/devstoreaccount1/" + mediaBlobStorageContainerName;// #spell-check-ignore-line
        }

        // Name property value could be different depending on the environment, so using the Data property instead.
        return dependencyTelemetry.Data.Contains(mediaBlobStorageContainerName);
    }
}
