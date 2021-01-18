namespace Lombiq.Hosting.Azure.ApplicationInsights
{
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
        /// Gets or sets a value indicating whether to enable a middleware that'll produce log entries on every request,
        /// as a test.
        /// </summary>
        public bool EnableLoggingTestMiddleware { get; set; }

        /// <summary>
        /// Gets or sets the API key to authenticate the controls channel for Quick Pulse (Live Metrics Stream). See
        /// the documentation for more info: <see
        /// href="https://docs.microsoft.com/en-us/azure/azure-monitor/app/live-stream#secure-the-control-channel"/>.
        /// </summary>
        public string QuickPulseTelemetryModuleAuthenticationApiKey { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to collect authenticated user's user name, if available, on every
        /// request. Note that the user agent might be sensitive personally identifiable information (PII); see the
        /// official documentation on handling PII: <see
        /// href="https://docs.microsoft.com/en-us/azure/azure-monitor/platform/personal-data-mgmt"/>.
        /// </summary>
        public bool EnableUserNameCollection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to collect the browser user agent on every request. Note that the
        /// user agent might be sensitive personally identifiable information (PII); see the official documentation on
        /// handling PII: <see
        /// href="https://docs.microsoft.com/en-us/azure/azure-monitor/platform/personal-data-mgmt"/>.
        /// </summary>
        public bool EnableUserAgentCollection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to collect the client IP address on every request. Note that the IP
        /// address might be sensitive personally identifiable information (PII); see the official documentation on
        /// handling PII: <see
        /// href="https://docs.microsoft.com/en-us/azure/azure-monitor/platform/personal-data-mgmt"/>.
        /// </summary>
        public bool EnableIpAddressCollection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to collect telemetry about Orchard background tasks.
        /// </summary>
        public bool EnableBackgroundTaskTelemetryCollection { get; set; } = true;
    }
}
