using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;

internal class ShellNamePopulatingTelemetryInitializer : ITelemetryInitializer
{
    private readonly IServiceProvider _serviceProvider;

    public ShellNamePopulatingTelemetryInitializer(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is not ISupportProperties supportProperties) return;

        var httpContext = _serviceProvider.GetHttpContextSafely();
        if (httpContext != null)
        {
            var httpRequest = httpContext.Request;

            var shellName = _serviceProvider
                .GetService<IRunningShellTable>()
                ?.Match(httpRequest.Host, httpRequest.PathBase + httpRequest.Path)
                ?.Name;

            if (!string.IsNullOrEmpty(shellName))
            {
                supportProperties.TryAddProperty("ShellName", shellName);
            }
        }
    }
}
