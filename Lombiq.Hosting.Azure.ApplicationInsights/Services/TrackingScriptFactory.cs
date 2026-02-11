using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.AspNetCore.Html;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class TrackingScriptFactory : ITrackingScriptFactory
{
    private readonly JavaScriptSnippet _javaScriptSnippet;
    private readonly IEnumerable<ITrackingScriptFactoryAddition> _trackingScriptFactoryAdditions;

    public TrackingScriptFactory(JavaScriptSnippet javaScriptSnippet, IEnumerable<ITrackingScriptFactoryAddition> trackingScriptFactoryAdditions)
    {
        _javaScriptSnippet = javaScriptSnippet;
        _trackingScriptFactoryAdditions = trackingScriptFactoryAdditions;
    }

    // The operation ID is NOT available in the injectable TelemetryClient, which will be basically empty. This is
    // somehow by design. See e.g.:
    // https://stackoverflow.com/questions/39149815/when-can-i-get-an-application-insights-operation-id
    public HtmlString CreateJavaScriptTrackingScript(bool enableCookies = true)
    {
        var script = _javaScriptSnippet.ScriptBody;

        if (!enableCookies)
        {
            if (!script.ContainsOrdinalIgnoreCase("cfg: {"))
            {
                throw new InvalidOperationException(
                    $"The JavaScript snippet should contain a configuration object (cfg) to disable cookies, but it was not found. Script: {script}");
            }

            script = script.ReplaceOrdinalIgnoreCase("cfg: {", "cfg: { disableCookiesUsage: true,");
        }

        return new(
            $@"<script>
                {script}
                appInsights.queue.push(function () {{
                    appInsights.context.telemetryTrace.traceID = '{System.Diagnostics.Activity.Current.RootId}';
                }});
                {string.Join('\n', _trackingScriptFactoryAdditions.Select(addition => addition.AddToJavaScriptTracking()))}
            </script>");
    }
}
