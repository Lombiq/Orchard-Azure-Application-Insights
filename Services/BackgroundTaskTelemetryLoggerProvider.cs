using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Logger Provider to create a fake logger to hook into the execution of a background tasks.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the only reliable way of doing it. Decorating <see cref="IBackgroundTask"/> will change task names
    /// (since that's always the type name) and cause only a single task to be executed, since they're referenced by
    /// their names.
    /// </para>
    /// <para>
    /// Note that this class and <see cref="BackgroundTaskTelemetryLogger"/> will in effect be a singleton per tenant.
    /// </para>
    /// </remarks>
    internal sealed class BackgroundTaskTelemetryLoggerProvider : ILoggerProvider
    {
        private static readonly NullLogger _nullLogger = new();

        private readonly IServiceProvider _serviceProvider;

        private BackgroundTaskTelemetryLogger _logger;

        public BackgroundTaskTelemetryLoggerProvider(IServiceProvider serviceProvider) =>
            _serviceProvider = serviceProvider;

        public ILogger CreateLogger(string categoryName)
        {
            if (!categoryName.Equals("OrchardCore.Modules.ModularBackgroundService", StringComparison.Ordinal))
            {
                return _nullLogger;
            }

            return _logger ??= new BackgroundTaskTelemetryLogger(_serviceProvider.GetRequiredService<TelemetryClient>());
        }

        public void Dispose() => _logger?.Dispose();

        private sealed class BackgroundTaskTelemetryLogger : ILogger, IDisposable
        {
            private readonly ConcurrentDictionary<string, IOperationHolder<DependencyTelemetry>> _operations = new();

            private readonly TelemetryClient _telemetryClient;

            public BackgroundTaskTelemetryLogger(TelemetryClient telemetryClient) => _telemetryClient = telemetryClient;

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (state is not IEnumerable<KeyValuePair<string, object>> backgroundState) return;

                string GetTaskName() => backgroundState.Single(parameter => parameter.Key == "TaskName").Value.ToString();

                var logMessage = formatter(state, exception);

                if (logMessage.StartsWith("Start processing background task", StringComparison.InvariantCultureIgnoreCase))
                {
                    var taskName = GetTaskName();

                    // Tried with a custom operation too, see c13dfdc47543d8635572be1f2ce1191dba06469b, but it remained
                    // "unconfigured", i.e. lacking the instrumentation key in the Context, and never getting sent to Azure.
                    // Note that while the operation is DependencyTelemetry it'll be collected even if
                    // EnableDependencyTrackingTelemetryModule is false. Also, this is the recommended way of tracking
                    // background tasks, see:
                    // https://docs.microsoft.com/en-us/azure/azure-monitor/app/custom-operations-tracking#long-running-background-tasks.
                    var operation = _telemetryClient.StartOperation<DependencyTelemetry>(taskName);
                    operation.Telemetry.Type = "BackgroundTask";

                    _operations.AddOrUpdate(
                        taskName,
                        operation,
                        (key, previousOperation) =>
                        {
                            // This should never actually run.
                            previousOperation.Dispose();
                            return operation;
                        });
                }
                else if (
                    (logMessage.StartsWith("Finished processing background task", StringComparison.InvariantCultureIgnoreCase) ||
                        logMessage.StartsWith("Error while processing background task", StringComparison.InvariantCultureIgnoreCase)) &&
                        _operations.TryRemove(GetTaskName(), out var operation))
                {
                    // Exceptions logged via the standard loggers will be properly correlated with the background
                    // operation without having to explicitly track them with TrackException().

                    operation.Dispose();
                }
            }

            public void Dispose()
            {
                foreach (var operation in _operations) operation.Value.Dispose();
            }
        }

        private class NullLogger : ILogger
        {
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => false;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                // Nothing to do.
            }
        }
    }
}
