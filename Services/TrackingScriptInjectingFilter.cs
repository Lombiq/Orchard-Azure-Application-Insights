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
        private readonly ITrackingScriptFactory _trackingScriptFactory;

        public TrackingScriptInjectingFilter(
            IResourceManager resourceManager,
            IOptions<ApplicationInsightsOptions> applicationInsightsOptions,
            ITrackingScriptFactory trackingScriptFactory)
        {
            _resourceManager = resourceManager;
            _applicationInsightsOptions = applicationInsightsOptions;
            _trackingScriptFactory = trackingScriptFactory;
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
                _resourceManager.RegisterHeadScript(_trackingScriptFactory.CreateJavaScriptTrackingScript());
            }

            await next();
        }
    }
}
