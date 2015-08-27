using System;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment;
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
            Work<ITelemetryConfigurationFactory> telemetryConfigurationFactoryWork)
        {
            _telemetrySettingsAccessorWork = telemetrySettingsAccessorWork;
            _telemetryConfigurationFactoryWork = telemetryConfigurationFactoryWork;


            OnLoaded<AzureApplicationInsightsTelemetrySettingsPart>((ctx, part) =>
                ctx.ContentItem.SetContext("PreviousInstrumentationKey", part.InstrumentationKey));

            OnUpdated<AzureApplicationInsightsTelemetrySettingsPart>((ctx, part) =>
                {
                    var previousInstrumentationKey = ctx.ContentItem.GetContext<string>("PreviousInstrumentationKey");
                    if (!string.IsNullOrEmpty(previousInstrumentationKey) && previousInstrumentationKey != part.InstrumentationKey)
                    {
                        Clear();
                    }
                });
        }


        public TelemetryConfiguration GetCurrentConfiguration()
        {
            lock (_lock)
            {
                if (_defaultConfiguration == null)
                {
                    var instrumentationKey = _telemetrySettingsAccessorWork.Value.GetCurrentSettings().InstrumentationKey;
                    if (string.IsNullOrEmpty(instrumentationKey)) return null;
                    _defaultConfiguration = _telemetryConfigurationFactoryWork.Value.CreateConfiguration(instrumentationKey);
                }

                return _defaultConfiguration;
            }
        }

        public void Dispose()
        {
            Clear();
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