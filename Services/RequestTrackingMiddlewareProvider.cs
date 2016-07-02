using System;
using System.Collections.Generic;
using Lombiq.Hosting.Azure.ApplicationInsights.Events;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Microsoft.ApplicationInsights.DataContracts;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Owin;
using Orchard.Services;
using Owin;
using Orchard.Exceptions;
using Orchard.Logging;
using Lombiq.Hosting.Azure.ApplicationInsights.Exceptions;
using Orchard.Environment.Configuration;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Owin middleware for tracking request metrics through Application Insights.
    /// </summary>
    public class RequestTrackingMiddlewareProvider : IOwinMiddlewareProvider
    {
        private readonly IWorkContextAccessor _wca;

        public ILogger Logger { get; set; }


        public RequestTrackingMiddlewareProvider(IWorkContextAccessor wca)
        {
            _wca = wca;

            Logger = NullLogger.Instance;
        }


        public IEnumerable<OwinMiddlewareRegistration> GetOwinMiddlewares()
        {
            return new[]
            {
                new OwinMiddlewareRegistration
                {
                    Configure = app =>
                        app.Use(async (context, next) =>
                        {
                            var nextDelegateWasRun = false;

                            try
                            {
                                var workContext = _wca.GetContext();

                                var requestTrackingIsEnabled = workContext.CurrentSite
                                    .As<AzureApplicationInsightsTelemetrySettingsPart>()
                                    .RequestTrackingIsEnabled;
                                if (requestTrackingIsEnabled)
                                {
                                    var clock = workContext.Resolve<IClock>();

                                    var requestStart = clock.UtcNow;

                                    var request = context.Request;
                                    var response = context.Response;
                                    var requestTrackingEvents = workContext.Resolve<IRequestTrackingEventHandler>();

                                    // Ideally filling most of the RequestTelemetry data wouldn't be required: once the
                                    // types in Microsoft.ApplicationInsights.Extensibility.Web.RequestTracking.TelemetryModules 
                                    // will be public they can be used instead of re-implementing them here.
                                    var requestTelemetry = new RequestTelemetry
                                    {
                                        Timestamp = requestStart,
                                        Url = request.Uri,
                                        HttpMethod = request.Method
                                    };
                                    requestTelemetry.Context.Location.Ip = 
                                        workContext.Resolve<IClientHostAddressAccessor>().GetClientAddress();
                                    if (request.Headers.ContainsKey("User-Agent"))
                                    {
                                        requestTelemetry.Context.User.UserAgent = request.Headers["User-Agent"];
                                    }
                                    requestTelemetry.Context.Operation.Id = requestTelemetry.Id;
                                    requestTelemetry.Properties[Constants.ShellNameKey] = 
                                        workContext.Resolve<ShellSettings>().Name;

                                    if (context.Environment.ContainsKey("System.Web.HttpContextBase"))
                                    {
                                        var httpContext = 
                                            context.Environment["System.Web.HttpContextBase"] as System.Web.HttpContextBase;
                                        if (httpContext != null)
                                        {
                                            var routeDataValues = httpContext.Request.RequestContext.RouteData.Values;
                                            if (routeDataValues.ContainsKey("controller"))
                                            {
                                                requestTelemetry.Name = request.Method + " " +
                                                    (routeDataValues["action"] == null ? "api/" : string.Empty) +
                                                    routeDataValues["area"] + "/" +
                                                    routeDataValues["controller"] + "/" +
                                                    routeDataValues["action"];
                                            }

                                            httpContext.Items[Constants.RequestIdKey] = requestTelemetry.Context.Operation.Id;
                                        }
                                    }

                                    requestTrackingEvents.BeginRequest(requestTelemetry);


                                    nextDelegateWasRun = true;
                                    try
                                    {
                                        await next.Invoke();
                                    }
                                    catch (Exception ex)
                                    {
                                        if (ex.IsFatal()) throw;
                                        throw new RequestException(ex);
                                    }


                                    requestTelemetry.Duration = clock.UtcNow - requestStart;
                                    requestTelemetry.ResponseCode = response.StatusCode.ToString();
                                    requestTelemetry.Success = response.StatusCode < 400;

                                    if (string.IsNullOrEmpty(requestTelemetry.Name))
                                    {
                                        requestTelemetry.Name = 
                                            (string)workContext.Layout.Title.ToString() ?? 
                                            request.Uri.ToString();
                                    }

                                    requestTrackingEvents.EndRequest(requestTelemetry);

                                    var telemetryClient = workContext
                                        .Resolve<ITelemetryClientFactory>()
                                        .CreateTelemetryClientFromCurrentConfiguration();
                                    if (telemetryClient != null)
                                    {
                                        telemetryClient.TrackRequest(requestTelemetry);
                                    }
                                }
                                else
                                {
                                    nextDelegateWasRun = true;
                                    await next.Invoke();
                                }
                            }
                            catch(RequestException)
                            {
                                // Let such exceptions bubble up, as these originate from the request pipeline downwards.
                                throw;
                            }
                            catch (Exception ex)
                            {
                                if (ex.IsFatal()) throw;
                                Logger.Error(ex, "An error happened during Application Insights request tracking. The problem most possibly completely prevents request tracking.");
                            }

                            // This should be rather in the catch, but there can't be an awaited call.
                            if (!nextDelegateWasRun)
                            {
                                await next.Invoke();
                            }
                        })
                }
            };
        }
    }
}