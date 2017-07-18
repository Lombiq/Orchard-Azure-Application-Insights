
namespace Lombiq.Hosting.Azure.ApplicationInsights
{
    public static class Constants
    {
        /// <summary>
        /// ID of the site settings editor group where AI settings are displayed.
        /// </summary>
        public const string SiteSettingsEditorGroup = "AzureApplicationInsightsSettings";

        /// <summary>
        /// Default name for the AI Log4Net appender.
        /// </summary>
        public const string DefaultLogAppenderName = "ai-appender";

        /// <summary>
        /// Configuration key for the default instrumentation key. The default instrumentation key is used if none is 
        /// saved in the database.
        /// </summary>
        public const string DefaultInstrumentationKeyConfigurationKey = "Lombiq.Hosting.Azure.ApplicationInsights.DefaultInstrumentationKey";

        /// <summary>
        /// Configuration key for the default API key. The default API key is used if none is saved in the database.
        /// </summary>
        public const string DefaultApiKeyConfigurationKey = "Lombiq.Hosting.Azure.ApplicationInsights.DefaultApiKey";
    }
}