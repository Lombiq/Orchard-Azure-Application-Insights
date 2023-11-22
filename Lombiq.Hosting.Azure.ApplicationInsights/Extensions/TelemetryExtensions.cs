using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Extensions;

public static class TelemetryExtensions
{
    public static void SetAsIgnoredFailure(this OperationTelemetry operationTelemetry)
    {
        operationTelemetry.Success = true;

        // Allow us to filter these requests in the portal:
        operationTelemetry.Properties["IgnoredFailure"] = "true";
    }

    public static bool ShouldSetAsIgnoredFailure(this DependencyTelemetry dependencyTelemetry, IServiceProvider serviceProvider) =>
        ShouldSetAsIgnoredFailure(dependencyTelemetry.ResultCode, dependencyTelemetry.Data, serviceProvider);

    public static bool ShouldSetAsIgnoredFailure(this RequestTelemetry requestTelemetry, IServiceProvider serviceProvider) =>
        ShouldSetAsIgnoredFailure(requestTelemetry.ResponseCode, requestTelemetry.Url.ToString(), serviceProvider);

    private static bool ShouldSetAsIgnoredFailure(string code, string data, IServiceProvider serviceProvider) =>
        int.TryParse(code, out var resultCode) &&
        resultCode is >= 400 and < 500 &&
        serviceProvider.GetRequiredService<IOptions<ApplicationInsightsOptions>>().Value
            .IgnoreFailureRegex
            .IsMatch(data);
}
