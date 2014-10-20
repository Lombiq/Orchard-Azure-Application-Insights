using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Drivers
{
    public class AzureApplicationInsightsTelemetryConfigurationPartDriver : ContentPartDriver<AzureApplicationInsightsTelemetryConfigurationPart>
    {
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
                        updater.TryUpdateModel(part, Prefix, null, null);
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