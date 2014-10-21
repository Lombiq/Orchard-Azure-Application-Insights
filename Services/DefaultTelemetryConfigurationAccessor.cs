using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard;
using Orchard.Settings;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment;
using Piedone.HelpfulLibraries.Contents;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Provides access to the default <see cref="TelemetryConfiguration"/> that can be modified from the admin.
    /// </summary>
    public interface IDefaultTelemetryConfigurationAccessor : ISingletonDependency
    {
        /// <summary>
        /// Returns the default <see cref="TelemetryConfiguration"/> that can be modified from the admin.
        /// </summary>
        TelemetryConfiguration GetDefaultConfiguration();
    }


    public class DefaultTelemetryConfigurationAccessor : ContentHandler, IDefaultTelemetryConfigurationAccessor, IDisposable
    {
        private object _lock = new object();

        private readonly Work<ISiteService> _siteServiceWork;
        private readonly Work<ITelemetryConfigurationFactory> _telemetryConfigurationFactoryWork;
        private TelemetryConfiguration _defaultConfiguration;


        public DefaultTelemetryConfigurationAccessor(
            Work<ISiteService> siteServiceWork,
            Work<ITelemetryConfigurationFactory> telemetryConfigurationFactoryWork)
        {
            _siteServiceWork = siteServiceWork;
            _telemetryConfigurationFactoryWork = telemetryConfigurationFactoryWork;


            OnLoaded<AzureApplicationInsightsTelemetryConfigurationPart>((ctx, part) =>
                ctx.ContentItem.Weld<AzureApplicationInsightsPreviousInstrumentationKeyPart>(p => p.PreviousInstrumentationKey = part.InstrumentationKey));

            OnUpdated<AzureApplicationInsightsTelemetryConfigurationPart>((ctx, part) =>
                {
                    var previousKeyPart = part.As<AzureApplicationInsightsPreviousInstrumentationKeyPart>();
                    if (previousKeyPart != null && previousKeyPart.PreviousInstrumentationKey != part.InstrumentationKey)
                    {
                        Clear();
                    }
                });
        }


        public TelemetryConfiguration GetDefaultConfiguration()
        {
            lock (_lock)
            {
                if (_defaultConfiguration == null)
                {
                    var instrumentationKey = _siteServiceWork.Value.GetSiteSettings().As<AzureApplicationInsightsTelemetryConfigurationPart>().InstrumentationKey;
                    if (string.IsNullOrEmpty(instrumentationKey))
                    {
                        throw new InvalidOperationException("The Application Insights instrumentation key is not configured. Without the instrumentation key no telemetry data can be sent.");
                    }
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


        /// <summary>
        /// Used to shortly store the instrumentation key when site settings are updated, so it can be detected if the key was changed.
        /// </summary>
        private class AzureApplicationInsightsPreviousInstrumentationKeyPart : ContentPart
        {
            public string PreviousInstrumentationKey { get; set; }
        }
    }
}