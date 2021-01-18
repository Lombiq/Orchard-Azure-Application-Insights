using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using System;
using System.Collections.Generic;
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

            using var operation = telemetryClient.StartOperation<BackgroundOperationTelemetry>(_decorated.GetType().FullName);
            //operation.Telemetry.Type = "Background";

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

        private class BackgroundOperationTelemetry : OperationTelemetry
        {
            private BackgroundTaskData _data;

            public override string Id { get; set; }
            public override string Name { get; set; }
            public override bool? Success { get; set; }
            public override TimeSpan Duration { get; set; }
            public override IDictionary<string, double> Metrics => new Dictionary<string, double>();
            public override IDictionary<string, string> Properties => new Dictionary<string, string>();
            public override DateTimeOffset Timestamp { get; set; }
            public override TelemetryContext Context => new();
            public override string Sequence { get; set; }
            public override IExtension Extension { get; set; }

            private BackgroundTaskData Data =>
                LazyInitializer.EnsureInitialized(
                    ref _data,
                    () => new BackgroundTaskData
                    {
                        Duration = Duration,
                        Id = Id,
                        Metrics = Metrics,
                        Name = Name,
                        Properties = Properties,
                        Success = Success,
                    });

            public override ITelemetry DeepClone() => throw new NotSupportedException();

            public override void SerializeData(ISerializationWriter serializationWriter)
            {
                // Following the pattern of what's in e.g. DependencyTelemetry.
                _data = null;
                serializationWriter.WriteProperty(Data);
            }

            private class BackgroundTaskData : ISerializableWithWriter
            {
                public string Id { get; set; }
                public string Name { get; set; }
                public bool? Success { get; set; }
                public TimeSpan Duration { get; set; }
                public IDictionary<string, double> Metrics { get; set; } = new Dictionary<string, double>();
                public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

                public void Serialize(ISerializationWriter serializationWriter)
                {
                    serializationWriter.WriteProperty("Id", Id);
                    serializationWriter.WriteProperty("Name", Name);
                    serializationWriter.WriteProperty("Duration", Duration);
                    serializationWriter.WriteProperty("Success", Success);
                    serializationWriter.WriteProperty("Properties", Properties);
                    serializationWriter.WriteProperty("Metrics", Metrics);
                }
            }
        }
    }
}
