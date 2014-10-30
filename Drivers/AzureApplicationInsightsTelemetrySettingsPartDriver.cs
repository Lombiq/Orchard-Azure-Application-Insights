using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Drivers
{
    public class AzureApplicationInsightsTelemetrySettingsPartDriver : ContentPartDriver<AzureApplicationInsightsTelemetrySettingsPart>
    {
        private readonly Work<ILoggerSetup> _loggerSetupWork;


        public AzureApplicationInsightsTelemetrySettingsPartDriver(Work<ILoggerSetup> loggerSetupWork)
        {
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
                        var previousInstrumentationKey = part.InstrumentationKey;
                        var previousEnableLogCollection = part.ApplicationWideLogCollectionIsEnabled;

                        updater.TryUpdateModel(part, Prefix, null, null);

                        if (part.ApplicationWideLogCollectionIsEnabled &&
                            (previousEnableLogCollection != part.ApplicationWideLogCollectionIsEnabled && !string.IsNullOrEmpty(part.InstrumentationKey)) ||
                            previousInstrumentationKey != part.InstrumentationKey)
                        {
                            _loggerSetupWork.Value.SetupAiAppender(Constants.DefaultLogAppenderName, part.InstrumentationKey);
                        }
                        else if (!part.ApplicationWideLogCollectionIsEnabled)
                        {
                            _loggerSetupWork.Value.RemoveAiAppender(Constants.DefaultLogAppenderName);
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