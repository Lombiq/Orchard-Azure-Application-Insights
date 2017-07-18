using System;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using Orchard.Services;
using Piedone.HelpfulLibraries.Contents;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Provides access to the default <see cref="TelemetryConfiguration"/> that can be modified from the admin.
    /// </summary>
    public interface ITelemetryConfigurationAccessor : ISingletonDependency
    {
        /// <summary>
        /// Returns the <see cref="TelemetryConfiguration"/> that can be modified from the admin or set from static
        /// configuration.
        /// </summary>
        TelemetryConfiguration GetCurrentConfiguration();
    }


    public class TelemetryConfigurationAccessor : ContentHandler, ITelemetryConfigurationAccessor, IDisposable
    {
        private object _lock = new object();

        private readonly Work<ITelemetrySettingsAccessor> _telemetrySettingsAccessorWork;
        private readonly Work<ITelemetryConfigurationFactory> _telemetryConfigurationFactoryWork;
        private TelemetryConfiguration _defaultConfiguration;


        public TelemetryConfigurationAccessor(
            Work<ITelemetrySettingsAccessor> telemetrySettingsAccessorWork,
            Work<ITelemetryConfigurationFactory> telemetryConfigurationFactoryWork,
            ShellSettings shellSettings,
            Work<IAppWideSetup> appWideSetupWork)
        {
            _telemetrySettingsAccessorWork = telemetrySettingsAccessorWork;
            _telemetryConfigurationFactoryWork = telemetryConfigurationFactoryWork;


            OnLoaded<AzureApplicationInsightsTelemetrySettingsPart>((ctx, part) =>
                ctx.ContentItem.SetContext("PreviousAISettingsContent", part.Stringify()));

            OnUpdated<AzureApplicationInsightsTelemetrySettingsPart>((ctx, part) =>
                {
                    var previousAISettingsContent = ctx.ContentItem.GetContext<string>("PreviousAISettingsContent");
                    var settingsContent = part.Stringify();
                    // Checking whether the settings changed. We need this since this method is invoked when any site
                    // settings change.
                    if (!string.IsNullOrEmpty(previousAISettingsContent) && previousAISettingsContent != settingsContent)
                    {
                        Clear();

                        if (shellSettings.Name == ShellSettings.DefaultName)
                        {
                            appWideSetupWork.Value.SetupAppWideServices(
                                LoadConfiguration(),
                                _telemetrySettingsAccessorWork.Value.GetCurrentSettings().ApiKey,
                                part.ApplicationWideDependencyTrackingIsEnabled,
                                part.ApplicationWideLogCollectionIsEnabled);
                        }
                    }
                });
        }


        public TelemetryConfiguration GetCurrentConfiguration()
        {
            return LoadConfiguration();
        }

        public void Dispose()
        {
            Clear();
        }


        private TelemetryConfiguration LoadConfiguration()
        {
            lock (_lock)
            {
                if (_defaultConfiguration == null)
                {
                    var telemetrySettingsAccessor = _telemetrySettingsAccessorWork.Value;

                    // Despite a work context existing the settings accessor sometimes can't be resolved for Web API
                    // requests.
                    if (telemetrySettingsAccessor == null) return null;

                    var instrumentationKey = telemetrySettingsAccessor.GetCurrentSettings().InstrumentationKey;
                    if (string.IsNullOrEmpty(instrumentationKey)) return null;
                    _defaultConfiguration = _telemetryConfigurationFactoryWork.Value.CreateConfiguration(instrumentationKey);
                }

                return _defaultConfiguration;
            }
        }
        private void Clear()
        {
            lock (_lock)
            {
                if (_defaultConfiguration == null) return;
                _defaultConfiguration.Dispose();
                _defaultConfiguration = null;
            }
        }
    }
}