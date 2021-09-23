using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
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
        private readonly IHttpContextAccessor _hca;

        public TrackingScriptInjectingFilter(
            IResourceManager resourceManager,
            IOptions<ApplicationInsightsOptions> applicationInsightsOptions,
            ITrackingScriptFactory trackingScriptFactory,
            IHttpContextAccessor hca)
        {
            _resourceManager = resourceManager;
            _applicationInsightsOptions = applicationInsightsOptions;
            _trackingScriptFactory = trackingScriptFactory;
            _hca = hca;
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.IsNotFullViewRendering() || !_applicationInsightsOptions.Value.EnableClientSideTracking)
            {
                await next();
                return;
            }

            var trackingConsentFeature = _hca.HttpContext.Features.Get<ITrackingConsentFeature>();

            if (trackingConsentFeature is null || trackingConsentFeature.CanTrack)
            {
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
            }

            // In the else branch we could delete the cookie that allows client side tracking.
            // These provide a solution to handle the following cases:
            // -The configuration of ITrackingConsentFeature changes after this module is enabled.(e.g. Lombiq.Privacy)
            // -A browser is used by multiple users and not all of them have accepted the privacy policy.

            await next();
        }
    }
}
