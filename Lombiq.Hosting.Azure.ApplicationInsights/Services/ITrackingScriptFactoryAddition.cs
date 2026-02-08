using Microsoft.AspNetCore.Html;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

/// <summary>
/// An interface for adding additional scripts to the Application Insights JavaScript tracking script that is injected
/// into the page.
/// </summary>
public interface ITrackingScriptFactoryAddition
{
    /// <summary>
    /// Possibility to insert additional scripts to the Application Insights JavaScript tracking script that is injected
    /// into the page.
    /// </summary>
    /// <example>
    /// <para>
    /// To exclude a specific exception from being sent to Application Insights, you can use the following code snippet
    /// returned as a <see cref="HtmlString"/>.
    /// </para>
    /// <code>
    /// const filteringFunction = (exception) => {
    ///   if (exception.data.url === "https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/js/bootstrap.bundle.js" &amp;&amp;
    ///     exception.data.message === "Uncaught TypeError: Cannot read properties of null (reading 'classList')") {
    ///       return false;
    ///   }
    ///   return true;
    /// };
    /// appInsights.addTelemetryInitializer(filteringFunction);
    /// </code>
    /// </example>
    public HtmlString AddToJavaScriptTracking();
}
