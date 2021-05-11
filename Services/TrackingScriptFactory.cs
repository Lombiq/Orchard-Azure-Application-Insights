using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Html;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    public class TrackingScriptFactory : ITrackingScriptFactory
    {
        private readonly JavaScriptSnippet _javaScriptSnippet;

        public TrackingScriptFactory(JavaScriptSnippet javaScriptSnippet) =>
            _javaScriptSnippet = javaScriptSnippet;

        // The operation ID is NOT available in the injectable TelemetryClient, which will be basically empty. This
        // is somehow by design. See e.g.:
        // https://stackoverflow.com/questions/39149815/when-can-i-get-an-application-insights-operation-id
        public HtmlString CreateJavaScriptTrackingScript() => new(
            $@"<script>
                {_javaScriptSnippet.ScriptBody}
                appInsights.queue.push(function () {{
                    appInsights.context.telemetryTrace.traceID = '{System.Diagnostics.Activity.Current.RootId}';
                }});
            </script>");
    }
}
