using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard;
using Orchard.Services;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers
{
    public class ContextPopulatingTelemetryInitializer : ITelemetryInitializer
    {
        public void Initialize(ITelemetry telemetry)
        {
            var request = HttpContext.Current?.Request;

            if (request == null) return;

            var workContext = request.RequestContext.GetWorkContext();

            if (workContext == null) return;

            var clientAddress = workContext.Resolve<IClientHostAddressAccessor>().GetClientAddress();
            // The address can be in the format IP:port.
            telemetry.Context.Location.Ip = clientAddress.Split(new[] { ':' })[0];

            var userAgent = request.Headers["User-Agent"];
            if (userAgent != null)
            {
                telemetry.Context.User.UserAgent = userAgent;
            }
            
            telemetry.Context.User.AuthenticatedUserId = workContext.GetAuthenticatedUserIdForRequest();
        }
    }
}