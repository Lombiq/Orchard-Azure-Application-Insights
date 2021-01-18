using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    internal class BackgroundTaskTelemetryDecorator : IBackgroundTask
    {
        private readonly IBackgroundTask _decorated;

        public BackgroundTaskTelemetryDecorator(IBackgroundTask decorated) => _decorated = decorated;

        public async Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            var telemetryClient = serviceProvider.GetRequiredService<TelemetryClient>();

            // Tried with a custom operation too, see c13dfdc47543d8635572be1f2ce1191dba06469b, but it remained
            // "unconfigured", i.e. lacking the instrumentation key in the Context, and never getting sent to Azure.
            // Note that while the operation is DependencyTelemetry it'll be collected even if
            // EnableDependencyTrackingTelemetryModule is false. Also, this is the recommended way of tracking
            // background tasks, see:
            // https://docs.microsoft.com/en-us/azure/azure-monitor/app/custom-operations-tracking#long-running-background-tasks.
            using var operation = telemetryClient.StartOperation<DependencyTelemetry>(_decorated.GetType().FullName);
            operation.Telemetry.Type = "Background";

            try
            {
                await _decorated.DoWorkAsync(serviceProvider, cancellationToken);
            }
            catch (Exception ex) when (!ex.IsFatal())
            {
                // Need to catch the exception to correlate it with the background operation.
                telemetryClient.TrackException(ex);
            }
        }
    }
}
