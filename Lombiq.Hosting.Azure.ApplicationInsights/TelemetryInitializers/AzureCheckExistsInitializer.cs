using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Media.Azure;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;

public class AzureCheckExistsInitializer : ITelemetryInitializer
{
    private readonly IServiceProvider _serviceProvider;

    public AzureCheckExistsInitializer(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not DependencyTelemetry dependencyTelemetry || dependencyTelemetry.Success == true)
        {
            return;
        }

        if (dependencyTelemetry.ResultCode != "409" && dependencyTelemetry.Name != "BlobContainerClient.Create")
        {
            return;
        }

        var shellConfiguration = _serviceProvider.GetRequiredService<IShellConfiguration>();
        var mediaBlobStorageOptions = _serviceProvider.GetRequiredService<IOptions<MediaBlobStorageOptions>>().Value;
        var dataProtectionConnectionString = shellConfiguration
            .GetValue<string>("OrchardCore_DataProtection_Azure:ConnectionString");

        var dataProtectionContainerName = shellConfiguration
            .GetValue("OrchardCore_DataProtection_Azure:ContainerName", "dataprotection"); // #spell-check-ignore-line

        if (dataProtectionConnectionString.Contains("UseDevelopmentStorage=true"))
        {
            dataProtectionContainerName = "/devstoreaccount1/" + dataProtectionContainerName; // #spell-check-ignore-line
        }

        var mediaBlobStorageContainerName = mediaBlobStorageOptions.ContainerName;
        if (mediaBlobStorageOptions.ConnectionString.Contains("UseDevelopmentStorage=true"))
        {
            mediaBlobStorageContainerName = "/devstoreaccount1/" + mediaBlobStorageContainerName; // #spell-check-ignore-line
        }

        dependencyTelemetry.TryAddProperty("DataProtectionContainerName", dataProtectionContainerName);
        dependencyTelemetry.TryAddProperty("MediaBlobStorageContainerName", mediaBlobStorageContainerName);
    }
}
