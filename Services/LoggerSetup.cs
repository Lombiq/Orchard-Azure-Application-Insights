using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using Microsoft.ApplicationInsights.Log4NetAppender;
using Orchard;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Sets up an AI logger.
    /// </summary>
    public interface ILoggerSetup : IDependency
    {
        /// <summary>
        /// Sets up an AI Log4Net appender and registers it with Log4Net.
        /// </summary>
        /// <param name="appenderName">A name for the appender, for identification purposes.</param>
        /// <param name="instrumentationKey">The AI instrumentation key to use.</param>
        /// <returns>The newly contsructed <see cref="ApplicationInsightsAppender"/>.</returns>
        ApplicationInsightsAppender SetupAiAppender(string appenderName, string instrumentationKey);

        /// <summary>
        /// Removes the given appender from among the active Log4Net appenders.
        /// </summary>
        /// <param name="appenderName">The appender's name.</param>
        void RemoveAiAppender(string appenderName);
    }


    public class LoggerSetup : ILoggerSetup
    {
        public ApplicationInsightsAppender SetupAiAppender(string appenderName, string instrumentationKey)
        {
            var hierarchyRoot = GetHierarchyRoot();

            RemoveAiAppender(appenderName);

            var patternLayout = new PatternLayout
            {
                ConversionPattern = "%date [%thread] %-5level %logger - %P{Tenant} - %message%newline"
            };
            patternLayout.ActivateOptions();

            var aiAppender = new ApplicationInsightsAppender
            {
                Name = appenderName,
                InstrumentationKey = instrumentationKey,
                Layout = patternLayout
            };
            aiAppender.ActivateOptions();

            hierarchyRoot.AddAppender(aiAppender);

            return aiAppender;
        }

        public void RemoveAiAppender(string appenderName)
        {
            var hierarchyRoot = GetHierarchyRoot();
            var existingAiAppender = hierarchyRoot.Appenders.Cast<IAppender>().FirstOrDefault(appender => appender.Name == appenderName);
            if (existingAiAppender != null)
            {
                hierarchyRoot.RemoveAppender(existingAiAppender);
            }
        }


        public static Logger GetHierarchyRoot()
        {
            return ((Hierarchy)LogManager.GetRepository()).Root;
        }
    }
}