using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Orchard;
using Orchard.ContentManagement;
using Orchard.Settings;
using Orchard.Environment.Configuration;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    public interface ITelemetrySettingsAccessor : IDependency
    {
        ITelemetrySettings GetCurrentSettings();
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


        public ITelemetrySettings GetCurrentSettings()
        {
            var defaultInstrumentationKey = _appConfigurationAccessor
                .GetConfiguration(Constants.DefaultInstrumentationKeyConfigurationKey);
            var defaultAPIKey = _appConfigurationAccessor
                .GetConfiguration(Constants.DefaultApiKeyConfigurationKey);

            var settings = new TelemetrySettings(_siteService.GetSiteSettings().As<AzureApplicationInsightsTelemetrySettingsPart>());

            if (!string.IsNullOrEmpty(defaultInstrumentationKey) && string.IsNullOrEmpty(settings.InstrumentationKey))
            {
                settings.InstrumentationKey = defaultInstrumentationKey;
            }

            if (!string.IsNullOrEmpty(defaultAPIKey) && string.IsNullOrEmpty(settings.ApiKey))
            {
                settings.ApiKey = defaultAPIKey;
            }

            return settings;
        }


        private class TelemetrySettings : ITelemetrySettings
        {
            public string InstrumentationKey { get; set; }
            public string ApiKey { get; set; }


            public TelemetrySettings(ITelemetrySettings previousSettings)
            {
                InstrumentationKey = previousSettings.InstrumentationKey;
                ApiKey = previousSettings.ApiKey;
            }
        }
    }
}