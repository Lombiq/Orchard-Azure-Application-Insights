using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment;
using Orchard.Environment.Configuration;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Drivers
{
    public class AzureApplicationInsightsTelemetrySettingsPartDriver : ContentPartDriver<AzureApplicationInsightsTelemetrySettingsPart>
    {
        private readonly ShellSettings _shellSettings;
        private readonly Work<ITelemetrySettingsAccessor> _telemetrySettingsAccessorWork;
        private readonly Work<ILoggerSetup> _loggerSetupWork;


        public AzureApplicationInsightsTelemetrySettingsPartDriver(
            ShellSettings shellSettings,
            Work<ITelemetrySettingsAccessor> telemetrySettingsAccessorWork,
            Work<ILoggerSetup> loggerSetupWork)
        {
            _shellSettings = shellSettings;
            _telemetrySettingsAccessorWork = telemetrySettingsAccessorWork;
            _loggerSetupWork = loggerSetupWork;
        }


        protected override DriverResult Editor(AzureApplicationInsightsTelemetrySettingsPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AzureApplicationInsightsTelemetrySettingsPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_AzureApplicationInsightsTelemetrySettings_Edit",
                () =>
                {
                    if (updater != null)
                    {
                        var previousEnableLogCollection = part.ApplicationWideLogCollectionIsEnabled;

                        updater.TryUpdateModel(part, Prefix, null, null);

                        if (_shellSettings.Name == ShellSettings.DefaultName)
                        {
                            // The default settings could come from elsewhere, not just this settings part, so using
                            // the service.
                            var defaultInstrumenationKey = _telemetrySettingsAccessorWork.Value
                                .GetDefaultSettings()
                                .InstrumentationKey;

                            TelemetryConfiguration.Active.InstrumentationKey = defaultInstrumenationKey;

                            if (part.ApplicationWideLogCollectionIsEnabled)
                            {
                                _loggerSetupWork.Value.SetupAiAppender(Constants.DefaultLogAppenderName, defaultInstrumenationKey);
                            }
                            else
                            {
                                _loggerSetupWork.Value.RemoveAiAppender(Constants.DefaultLogAppenderName);
                            }
                        }
                    }

                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts.AzureApplicationInsightsTelemetrySettings",
                        Model: part,
                        Prefix: Prefix);
                })
            .OnGroup(Constants.SiteSettingsEditorGroup);
        }
    }
}