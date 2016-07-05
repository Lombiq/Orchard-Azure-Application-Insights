using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers
{
    /// <summary>
    /// Sets the operation ID for the telemetry context, if there is an ID in the http context.
    /// </summary>
    /// <remarks>
    /// The class's aim is basically the same as <see cref="Microsoft.ApplicationInsights.Extensibility.Web.TelemetryInitializers"/>
    /// but that class is internal and thus unusable.
    /// This is necessary especially for TelemetryConfiguration.Active, as that lives independently from our custom 
    /// request tracking.
    /// </remarks>
    public class WebOperationIdTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            if (HttpContext.Current == null) return;
            var requestId = HttpContext.Current.Items[Constants.RequestIdKey];
            if (requestId == null || !string.IsNullOrEmpty(telemetry.Context.Operation.Id)) return;
            telemetry.Context.Operation.Id = requestId as string;
        }
    }
}