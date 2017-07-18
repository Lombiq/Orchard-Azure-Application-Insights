using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard.Environment;
using Orchard.Mvc;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers
{
    /// <summary>
    /// Adds the shell's (tenant's) name to the telemetry as supplemental information.
    /// </summary>
    /// <remarks>
    /// Uses the same technique as <see cref="Orchard.Logging.OrchardLog4netLogger"/>.
    /// </remarks>
    public class ShellNameTelemetryInitializer : ITelemetryInitializer, IShim
    {
        public IOrchardHostContainer HostContainer { get; set; }


        public ShellNameTelemetryInitializer()
        {
            OrchardHostContainerRegistry.RegisterShim(this);
        }


        public void Initialize(ITelemetry telemetry)
        {
            var telemetryWithProperties = telemetry as ISupportProperties;

            if (telemetryWithProperties == null) return;


            // If already set, nothing to do.
            if (!string.IsNullOrEmpty(telemetryWithProperties.GetShellName())) return;


            // Below algorithm is mostly copied from OrchardLog4netLogger but uses IHttpContextAccessor.
            var httpContext = HostContainer.Resolve<IHttpContextAccessor>().Current();
            if (httpContext == null) return;

            var runningShellTable = HostContainer.Resolve<IRunningShellTable>();
            if (runningShellTable == null) return;

            var shellSettings = runningShellTable.Match(httpContext);
            if (shellSettings == null) return;

            var orchardHost = HostContainer.Resolve<IOrchardHost>();
            if (orchardHost == null) return;

            var shellContext = orchardHost.GetShellContext(shellSettings);
            if (shellContext == null || shellContext.Settings == null) return;


            telemetryWithProperties.SetShellName(shellContext.Settings);
        }
    }
}