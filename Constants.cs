using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.Hosting.Azure.ApplicationInsights
{
    public static class Constants
    {
        /// <summary>
        /// ID of the site settings editor group where AI settings are displayed.
        /// </summary>
        public const string SiteSettingsEditorGroup = "AzureApplicationInsightsConfiguration";

        /// <summary>
        /// Default name for the AI Log4Net appender.
        /// </summary>
        public const string DefaultAiLogAppenderName = "ai-appender";
    }
}