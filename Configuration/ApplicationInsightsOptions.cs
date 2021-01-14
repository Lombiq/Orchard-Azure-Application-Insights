namespace Lombiq.Hosting.Azure.ApplicationInsights.Configuration
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
    }
}
