using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard.Environment;
using Orchard.Mvc;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers
{
    /// <summary>
    /// Sets the operation ID for the telemetry context, if there is an ID in the HTTP context.
    /// </summary>
    /// <remarks>
    /// The class's aim is basically the same as <see cref="Microsoft.ApplicationInsights.Extensibility.Web.TelemetryInitializers"/>
    /// but that class is internal and thus unusable.
    /// This is necessary especially for TelemetryConfiguration.Active, as that lives independently from our custom 
    /// request tracking.
    /// </remarks>
    public class WebOperationIdTelemetryInitializer : ITelemetryInitializer, IShim
    {
        public IOrchardHostContainer HostContainer { get; set; }


        public WebOperationIdTelemetryInitializer()
        {
            OrchardHostContainerRegistry.RegisterShim(this);
        }


        public void Initialize(ITelemetry telemetry)
        {
            var requestId = HostContainer.Resolve<IHttpContextAccessor>().Current()?.GetOperationIdForRequest();
            if (requestId == null || !string.IsNullOrEmpty(telemetry.Context.Operation.Id)) return;
            telemetry.Context.Operation.Id = requestId as string;
        }
    }
}