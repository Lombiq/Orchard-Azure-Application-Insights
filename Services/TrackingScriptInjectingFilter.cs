using Lombiq.Hosting.Azure.ApplicationInsights.Constants;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Filters;
using OrchardCore.ResourceManagement;
using System.Threading.Tasks;
namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    public class TrackingScriptInjectingFilter : IAsyncResultFilter
    {
        private readonly IResourceManager _resourceManager;

        public TrackingScriptInjectingFilter(IResourceManager resourceManager) => _resourceManager = resourceManager;

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.IsNotFullViewRendering()) await next();

            _resourceManager.RegisterResource("script", ResourceNames.TrackingScript).AtHead();

            // The operation ID is NOT available in the injectable TelemetryClient, which will be basically empty. This
            // is somehow by design. See e.g.:
            // https://stackoverflow.com/questions/39149815/when-can-i-get-an-application-insights-operation-id
            var operationId = System.Diagnostics.Activity.Current.RootId;

            var correlationScript = new HtmlString(
                $@"<script>
                    appInsights.queue.push(function () {{
                        appInsights.properties.context.telemetryTrace.traceID = '{operationId}';
                    }});
                </script>");
            _resourceManager.RegisterHeadScript(correlationScript);
            await next();
        }
    }
}
