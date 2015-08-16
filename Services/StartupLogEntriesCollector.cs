using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using log4net.Appender;
using Orchard;
using Orchard.Environment.Configuration;
using Orchard.Logging;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Service for dealing with log entries that were logged during the startup of the application.
    /// </summary>
    public interface IStartupLogEntriesCollector : IDependency
    {
        /// <summary>
        /// Re-logs those log entries (if not already done) that where created during application start, before the AI 
        /// appender was registered.
        /// </summary>
        void ReLogStartupLogEntriesIfNew();
    }


    public class StartupLogEntriesCollector : IStartupLogEntriesCollector
    {
        private static object _reLogLock = new object();
        private static bool _wasReLogged = false;

        private readonly ShellSettings _shellSettings;

        public ILogger Logger { get; set; }


        public StartupLogEntriesCollector(ShellSettings shellSettings)
        {
            _shellSettings = shellSettings;

            Logger = NullLogger.Instance;
        }


        public void ReLogStartupLogEntriesIfNew()
        {
            // The AI appender, being database-configured, is registered after the app (and shell) startup. Because of 
            // this here we try to access the standard Orchard log file, read the entries that were logged since the app 
            // start and send them to AI.
            // Note that this will only work if the log entries were already flushed (written to the file) which can be 
            // only ensured with immediate flushing.
            if (_shellSettings.Name == ShellSettings.DefaultName && !_wasReLogged)
            {
                // This shouldn't happen from more threads at the same time but let's be sure.
                lock (_reLogLock)
                {
                    // We won't re-log if the tenant was just restarted. This can happens even when toggling features so 
                    // we have to check.
                    _wasReLogged = true;

                    var hierarchyRoot = LoggerSetup.GetHierarchyRoot();

                    var orchardAppender = hierarchyRoot.Appenders
                        .Cast<IAppender>()
                        .FirstOrDefault(appender => appender.GetType().Name == "OrchardFileAppender");
                    if (orchardAppender != null)
                    {
                        // Trying to access the existing log file.
                        var logFilePath = ((Orchard.Logging.OrchardFileAppender)orchardAppender).File;
                        if (File.Exists(logFilePath))
                        {
                            // Reading the log file line by line, trying to retrieve log entries created since the app 
                            // start, then re-logging them.
                            // This won't cut it if the log file was changed just during startup (because e.g. midnight 
                            // passed) but good enough.
                            using (var streamReader = File.OpenText(logFilePath))
                            {
                                // Checking for lines that appear to be starts of log entries, then check for those that 
                                // were made after the start of the worker process (this is an approximation to get 
                                // entries that were logged after the app start; actually the app can be restarted while
                                // the worker process keeps running.

                                var startTime = Process.GetCurrentProcess().StartTime;
                                string line;
                                while ((line = streamReader.ReadLine()) != null)
                                {
                                    var segments = line.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                                    DateTime logTime;

                                    if (segments.Length > 2 &&
                                        (DateTime.TryParse(segments[0] + " " + segments[1].Split(',')[0], out logTime) 
                                            || DateTime.TryParse(segments[0], out logTime)) &&
                                        logTime > startTime)
                                    {
                                        var logMessage = 
                                            "Re-logging entries since app start for Application Insights. The following entries were previously logged but only to the log file, so re-logging them so they are sent to AI." +
                                            Environment.NewLine + line + Environment.NewLine + streamReader.ReadToEnd();

                                        // Re-logging with level of the logger. Re-logging is a workaround in itself 
                                        // where this is another one: the re-logging message from above can even have e.g.
                                        // the ERROR level, what is not accurate. Nevertheless this is to ensure that we 
                                        // use the lowest possible level, trying to avoid false alarms.
                                        var level = hierarchyRoot.EffectiveLevel.Name;
                                        if (level == log4net.Core.Level.Fatal.Name)
                                        {
                                            Logger.Fatal(logMessage);
                                        }
                                        else if (level == log4net.Core.Level.Error.Name)
                                        {
                                            Logger.Error(logMessage);
                                        }
                                        else if (level == log4net.Core.Level.Warn.Name)
                                        {
                                            Logger.Warning(logMessage);
                                        }
                                        else if (level == log4net.Core.Level.Info.Name)
                                        {
                                            Logger.Information(logMessage);
                                        }
                                        else // No more levels used in Orchard, this should be Debug.
                                        {
                                            Logger.Debug(logMessage);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}