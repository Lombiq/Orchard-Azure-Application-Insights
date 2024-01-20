using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class TrackingScriptInjectingFilter(
    IResourceManager resourceManager,
    IOptions<ApplicationInsightsOptions> applicationInsightsOptions,
    ITrackingScriptFactory trackingScriptFactory,
    IHttpContextAccessor hca) : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.IsNotFullViewRendering() || !applicationInsightsOptions.Value.EnableClientSideTracking)
        {
            await next();
            return;
        }

        var trackingConsentFeature = hca.HttpContext.Features.Get<ITrackingConsentFeature>();

        if (trackingConsentFeature is null || trackingConsentFeature.CanTrack)
        {
            if (applicationInsightsOptions.Value.EnableOfflineOperation)
            {
                var offlineScript = new HtmlString(
                    $@"<script>
                        appInsights = 'enabled';
                    </script>");
                resourceManager.RegisterHeadScript(offlineScript);
            }
            else
            {
                resourceManager.RegisterHeadScript(trackingScriptFactory.CreateJavaScriptTrackingScript());
            }
        }

        // In the else branch we could delete the cookie that allows client-side tracking. These provide a solution to
        // handle the following cases:
        // - The configuration of ITrackingConsentFeature changes after this module is enabled. (e.g. with
        //   Lombiq.Privacy: https://github.com/Lombiq/Orchard-Privacy)
        // - A browser is used by multiple users and not all of them have accepted the privacy policy.

        await next();
    }
}
