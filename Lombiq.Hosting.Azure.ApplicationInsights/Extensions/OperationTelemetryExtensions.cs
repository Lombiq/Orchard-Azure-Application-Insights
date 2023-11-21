using Microsoft.ApplicationInsights.Extensibility.Implementation;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Extensions;

public static class OperationTelemetryExtensions
{
    public static void SetAsIgnoredFailure(this OperationTelemetry operationTelemetry)
    {
        operationTelemetry.Success = true;

        // Allow us to filter these requests in the portal:
        operationTelemetry.Properties["IgnoredFailure"] = "true";
    }
}
