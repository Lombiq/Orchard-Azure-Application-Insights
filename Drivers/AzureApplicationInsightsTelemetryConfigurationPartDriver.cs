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
    public class AzureApplicationInsightsTelemetryConfigurationPartDriver : ContentPartDriver<AzureApplicationInsightsTelemetryConfigurationPart>
    {
        private readonly Work<ILoggerSetup> _loggerSetupWork;


        public AzureApplicationInsightsTelemetryConfigurationPartDriver(Work<ILoggerSetup> loggerSetupWork)
        {
            _loggerSetupWork = loggerSetupWork;
        }
        
        
        protected override DriverResult Editor(AzureApplicationInsightsTelemetryConfigurationPart part, dynamic shapeHelper)
        {
            return Editor(part, null, shapeHelper);
        }

        protected override DriverResult Editor(AzureApplicationInsightsTelemetryConfigurationPart part, IUpdateModel updater, dynamic shapeHelper)
        {
            return ContentShape("Parts_AzureApplicationInsightsTelemetryConfiguration_Edit",
                () =>
                {
                    if (updater != null)
                    {
                        var previousInstrumentationKey = part.InstrumentationKey;
                        var previousEnableLogCollection = part.EnableLogCollection;

                        updater.TryUpdateModel(part, Prefix, null, null);

                        if (part.EnableLogCollection &&
                            (previousEnableLogCollection != part.EnableLogCollection && !string.IsNullOrEmpty(part.InstrumentationKey)) ||
                            previousInstrumentationKey != part.InstrumentationKey)
                        {
                            _loggerSetupWork.Value.SetupAiAppender(Constants.DefaultAiLogAppenderName, part.InstrumentationKey);
                        }
                        else if (!part.EnableLogCollection)
                        {
                            _loggerSetupWork.Value.RemoveAiAppender(Constants.DefaultAiLogAppenderName);
                        }
                    }

                    return shapeHelper.EditorTemplate(
                        TemplateName: "Parts.AzureApplicationInsightsTelemetryConfiguration",
                        Model: part,
                        Prefix: Prefix);
                })
            .OnGroup(Constants.SiteSettingsEditorGroup);
        }
    }
}