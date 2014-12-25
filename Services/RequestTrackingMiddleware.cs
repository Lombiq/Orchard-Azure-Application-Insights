using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Logging;
using Orchard.Owin;
using Owin;
using Microsoft.Owin.Extensions;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Orchard;
using Orchard.Mvc;
using Orchard.Services;
using Microsoft.ApplicationInsights.DataContracts;
using Lombiq.Hosting.Azure.ApplicationInsights.Events;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Owin middleware for tracking request metrics through Application Insights.
    /// </summary>
    public class RequestTrackingMiddleware : IOwinMiddlewareProvider
    {
        private readonly IWorkContextAccessor _wca;


        public RequestTrackingMiddleware(IWorkContextAccessor wca)
        {
            _wca = wca;
        }


        public IEnumerable<OwinMiddleware> GetOwinMiddlewares()
        {
            return new[]
            {
                new OwinMiddleware
                {
                    Configure = app =>
                        {
                            app.Use(async (context, next) =>
                            {
                                var workContext = _wca.GetContext();
                                var clock = workContext.Resolve<IClock>();

                                var requestStart = clock.UtcNow;

                                var httpContext = workContext.Resolve<IHttpContextAccessor>().Current();
                                var httpRequest = httpContext.Request;
                                var httpResponse = httpContext.Response;
                                var requestTrackingEvents = workContext.Resolve<IRequestTrackingEventHandler>();


                                var requestTelemetry = new RequestTelemetry
                                {
                                    Timestamp = requestStart,
                                    Url = httpRequest.Url,
                                    HttpMethod = httpRequest.HttpMethod
                                };
                                requestTelemetry.Context.Location.Ip = httpRequest.UserHostAddress;

                                requestTrackingEvents.OnBeginRequest(requestTelemetry);

                                await next.Invoke();

                                requestTelemetry.Duration = clock.UtcNow - requestStart;
                                requestTelemetry.ResponseCode = httpResponse.StatusCode.ToString();
                                requestTelemetry.Success = httpResponse.StatusCode < 400;
                                requestTelemetry.Name = (string)workContext.Layout.Title ?? httpRequest.Url.ToString();

                                requestTrackingEvents.OnEndRequest(requestTelemetry);

                                workContext.Resolve<ITelemetryClientFactory>().CreateTelemetryClientFromDefaultConfiguration().TrackRequest(requestTelemetry);
                            });
                        }
                }
            };
        }
    }
}