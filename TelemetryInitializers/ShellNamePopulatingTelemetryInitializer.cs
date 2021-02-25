using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using OrchardCore.Environment.Shell;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers
{
    internal class ShellNamePopulatingTelemetryInitializer : ITelemetryInitializer
    {
        private readonly ShellSettings _shellSettings;

        public ShellNamePopulatingTelemetryInitializer(ShellSettings shellSettings) => _shellSettings = shellSettings;

        public void Initialize(ITelemetry telemetry)
        {
            if (telemetry is not ISupportProperties supportProperties) return;

            supportProperties.TryAddProperty("ShellName", _shellSettings.Name);
        }
    }
}
