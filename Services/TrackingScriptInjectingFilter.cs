using Lombiq.Hosting.Azure.ApplicationInsights.Constants;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    public class TrackingScriptInjectingFilter : IAsyncResultFilter
    {
        private readonly IResourceManager _resourceManager;
        private readonly IOptions<ApplicationInsightsOptions> _applicationInsightsOptions;

        public TrackingScriptInjectingFilter(
            IResourceManager resourceManager,
            IOptions<ApplicationInsightsOptions> applicationInsightsOptions)
        {
            _resourceManager = resourceManager;
            _applicationInsightsOptions = applicationInsightsOptions;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.IsNotFullViewRendering() || !_applicationInsightsOptions.Value.EnableClientSideTracking)
            {
                await next();
                return;
            }

            if (_applicationInsightsOptions.Value.EnableOfflineOperation)
            {
                var offlineScript = new HtmlString(
                    $@"<script>
                        appInsights = 'enabled';
                    </script>");
                _resourceManager.RegisterHeadScript(offlineScript);
            }
            else
            {
                _resourceManager.RegisterResource("script", ResourceNames.TrackingScript).AtHead();

                // The operation ID is NOT available in the injectable TelemetryClient, which will be basically
                // empty. This is somehow by design. See e.g.:
                // https://stackoverflow.com/questions/39149815/when-can-i-get-an-application-insights-operation-id
                var operationId = System.Diagnostics.Activity.Current.RootId;

                var correlationScript = new HtmlString(
                    $@"<script>
                        appInsights.queue.push(function () {{
                            appInsights.context.telemetryTrace.traceID = '{operationId}';
                        }});
                    </script>");
                _resourceManager.RegisterHeadScript(correlationScript);
            }

            await next();
        }
    }
}
