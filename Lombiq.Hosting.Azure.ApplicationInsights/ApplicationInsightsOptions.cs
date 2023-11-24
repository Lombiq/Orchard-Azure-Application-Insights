using Microsoft.ApplicationInsights.DataContracts;
using System;
using System.Text.RegularExpressions;

namespace Lombiq.Hosting.Azure.ApplicationInsights;

/// <summary>
/// Further configuration options for the module.
/// </summary>
public class ApplicationInsightsOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to collect SQL queries' command texts as well during dependency
    /// tracking. See: <see
    /// href="https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-dependencies#advanced-sql-tracking-to-get-full-sql-query"/>.
    /// </summary>
    public bool EnableSqlCommandTextInstrumentation { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable a middleware that'll produce log entries on requests
    /// containing the "logtest" query string parameter (i.e. "?logtest" or "&amp;logtest" being there in the URL), as a
    /// test.
    /// </summary>
    public bool EnableLoggingTestMiddleware { get; set; }

    /// <summary>
    /// Gets or sets the API key to authenticate the control channel for Quick Pulse (Live Metrics Stream). See the
    /// documentation for more info: <see
    /// href="https://docs.microsoft.com/en-us/azure/azure-monitor/app/live-stream#secure-the-control-channel"/>.
    /// </summary>
    public string QuickPulseTelemetryModuleAuthenticationApiKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to collect authenticated user's user name, if available, on every
    /// request. Note that the user name might be sensitive personally identifiable information (PII); see the official
    /// documentation on handling PII: <see
    /// href="https://docs.microsoft.com/en-us/azure/azure-monitor/platform/personal-data-mgmt"/>.
    /// </summary>
    public bool EnableUserNameCollection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to collect the browser user agent on every request. Note that the user
    /// agent might be sensitive personally identifiable information (PII); see the official documentation on handling
    /// PII: <see href="https://docs.microsoft.com/en-us/azure/azure-monitor/platform/personal-data-mgmt"/>.
    /// </summary>
    public bool EnableUserAgentCollection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to collect the client IP address on every request. Note that the IP
    /// address might be sensitive personally identifiable information (PII); see the official documentation on handling
    /// PII: <see href="https://docs.microsoft.com/en-us/azure/azure-monitor/platform/personal-data-mgmt"/>.
    /// </summary>
    public bool EnableIpAddressCollection { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to collect telemetry about Orchard background tasks.
    /// </summary>
    public bool EnableBackgroundTaskTelemetryCollection { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to enable a background task that'll produce log entries every minute.
    /// Entries will only show up in AI if <see cref="EnableBackgroundTaskTelemetryCollection"/> is also <see
    /// langword="true"/>.
    /// </summary>
    public bool EnableLoggingTestBackgroundTask { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to inject the client-side AI tracking script.
    /// </summary>
    public bool EnableClientSideTracking { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to work in kind of a debug mode completely offline. Telemetry will still
    /// show up in the Debug window.
    /// </summary>
    public bool EnableOfflineOperation { get; set; }

    /// <summary>
    /// Gets or sets <see cref="IgnoreFailureRegex"/> by compiling the given string into a regular expression.
    /// </summary>
    /// <example>You should use a regex pattern like "(?:\\/favicon.ico$)|(?:\\/media\\/)".</example>
    public string IgnoreFailureRegexPattern
    {
        get => IgnoreFailureRegex?.ToString();
        set => IgnoreFailureRegex = new Regex(value, RegexOptions.Compiled, TimeSpan.FromSeconds(1));
    }

    /// <summary>
    /// Gets or sets a regular expression that will be used to set telemetry to success if it matches
    /// <see cref="DependencyTelemetry.Data"/> or <see cref="RequestTelemetry.Url"/>. This is useful if
    /// you have a lot of 404s or other errors that you don't want to see as failures in Application Insights.
    /// </summary>
    public Regex IgnoreFailureRegex { get; private set; }
}
