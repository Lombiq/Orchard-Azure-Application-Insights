using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Extensions;

public static class TelemetryExtensions
{
    /// <summary>
    /// Sets the telemetry as an ignored failure. This means that it'll be marked as a success and the "IgnoredFailure"
    /// custom property will be added to it so that it can be filtered out in the portal.
    /// </summary>
    public static void SetAsIgnoredFailure(this OperationTelemetry operationTelemetry)
    {
        operationTelemetry.Success = true;

        // Allow us to filter these requests in the portal:
        operationTelemetry.Properties["IgnoredFailure"] = "true";
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="DependencyTelemetry"/> should be set as an ignored failure.
    /// Returns false otherwise.
    /// </summary>
    public static bool ShouldSetAsIgnoredFailure(this DependencyTelemetry dependencyTelemetry, IServiceProvider serviceProvider) =>
        ShouldSetAsIgnoredFailure(dependencyTelemetry.ResultCode, dependencyTelemetry.Data, serviceProvider);

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="RequestTelemetry"/> should be set as an ignored failure.
    /// Returns false otherwise.
    /// </summary>
    public static bool ShouldSetAsIgnoredFailure(this RequestTelemetry requestTelemetry, IServiceProvider serviceProvider) =>
        ShouldSetAsIgnoredFailure(requestTelemetry.ResponseCode, requestTelemetry.Url.ToString(), serviceProvider);

    private static bool ShouldSetAsIgnoredFailure(string code, string data, IServiceProvider serviceProvider) =>
        int.TryParse(code, out var resultCode) &&
        resultCode is >= 400 and < 500 &&
        serviceProvider.GetRequiredService<IOptions<ApplicationInsightsOptions>>().Value
            .IgnoreFailureRegex
            .IsMatch(data);
}
