using Microsoft.AspNetCore.Html;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Service for creating JavaScript tracking scripts for Application Insights.
    /// </summary>
    public interface ITrackingScriptFactory
    {
        /// <summary>
        /// Creates a JavaScript tracking scripts for Application Insights, including correlation to the server-side
        /// request.
        /// </summary>
        HtmlString CreateJavaScriptTrackingScript();
    }
}
