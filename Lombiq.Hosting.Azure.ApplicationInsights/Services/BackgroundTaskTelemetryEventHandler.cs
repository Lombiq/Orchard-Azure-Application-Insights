using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using OrchardCore.BackgroundTasks;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

internal sealed class BackgroundTaskTelemetryEventHandler(TelemetryClient telemetryClient) : IBackgroundTaskEventHandler
{
    private IOperationHolder<DependencyTelemetry> _operation;
    private Activity _activity;

    public Task ExecutingAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken)
    {
        // Tried with a custom operation too, see c13dfdc47543d8635572be1f2ce1191dba06469b, but it remained
        // "unconfigured", i.e. lacking the instrumentation key in the Context, and never getting sent to Azure. Note
        // that while the operation is DependencyTelemetry it'll be collected even if
        // EnableDependencyTrackingTelemetryModule is false. Also, this is the recommended way of tracking background
        // tasks, see:
        // https://docs.microsoft.com/en-us/azure/azure-monitor/app/custom-operations-tracking#long-running-background-tasks.
        var operation = telemetryClient.StartOperation<DependencyTelemetry>(context.Name);
        operation.Telemetry.Type = "BackgroundTask";

        _operation = operation;
        _activity = Activity.Current;

        return Task.CompletedTask;
    }

    public Task ExecutedAsync(BackgroundTaskEventContext context, CancellationToken cancellationToken)
    {
        if (_activity != null)
        {
            // Due to async context switches the original Activity is lost and Activity.Current would be null here.
            // Thus, we need to set it explicitly. Such operations aren't necessarily properly supported by AI, see:
            // https://docs.microsoft.com/en-us/azure/azure-monitor/app/custom-operations-tracking#parallel-operations-processing-and-tracking
            Activity.Current = _activity;
            _operation?.Dispose();
        }

        return Task.CompletedTask;
    }
}
