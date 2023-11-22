using Lombiq.Hosting.Azure.ApplicationInsights.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;

public class IgnoreFailureInitializer : ITelemetryInitializer
{
    private readonly IServiceProvider _serviceProvider;

    public IgnoreFailureInitializer(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public void Initialize(ITelemetry telemetry)
    {
        var operationTelemetry = telemetry as OperationTelemetry;
        if (operationTelemetry?.Success != false) return;

        if (operationTelemetry is RequestTelemetry requestTelemetry &&
            requestTelemetry.ShouldSetAsIgnoredFailure(_serviceProvider))
        {
            requestTelemetry.SetAsIgnoredFailure();
            return;
        }

        if (operationTelemetry is DependencyTelemetry dependencyTelemetry &&
            dependencyTelemetry.ShouldSetAsIgnoredFailure(_serviceProvider))
        {
            dependencyTelemetry.SetAsIgnoredFailure();
        }
    }
}
