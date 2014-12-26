using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Settings;
using Piedone.HelpfulLibraries.Utilities;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    public interface ITelemetrySettingsAccessor : IDependency
    {
        ITelemetrySettings GetDefaultSettings();
    }


    public class TelemetrySettingsAccessor : ITelemetrySettingsAccessor
    {
        private readonly IAppConfigurationAccessor _appConfigurationAccessor;
        private readonly ISiteService _siteService;


        public TelemetrySettingsAccessor(
            IAppConfigurationAccessor appConfigurationAccessor,
            ISiteService siteService)
        {
            _appConfigurationAccessor = appConfigurationAccessor;
            _siteService = siteService;
        }


        public ITelemetrySettings GetDefaultSettings()
        {
            var defaultInstrumentationKey = _appConfigurationAccessor.GetConfiguration(Constants.DefaultInstrumentationKeyConfigurationKey);
            var settings = _siteService.GetSiteSettings().As<AzureApplicationInsightsTelemetrySettingsPart>();
            if (!string.IsNullOrEmpty(defaultInstrumentationKey) && string.IsNullOrEmpty(settings.InstrumentationKey))
            {
                return new TelemetrySettings(settings) { InstrumentationKey = defaultInstrumentationKey };
            }

            return settings;
        }


        private class TelemetrySettings : ITelemetrySettings
        {
            public string InstrumentationKey { get; set; }
            public bool ApplicationWideLogCollectionIsEnabled { get; set; }


            public TelemetrySettings(ITelemetrySettings previousSettings)
            {
                InstrumentationKey = previousSettings.InstrumentationKey;
                ApplicationWideLogCollectionIsEnabled = previousSettings.ApplicationWideLogCollectionIsEnabled;
            }
        }
    }
}