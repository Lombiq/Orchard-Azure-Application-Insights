using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Drivers;
using Orchard.Environment;
using Orchard.Environment.Configuration;
using System.Linq;
using Piedone.HelpfulLibraries.Libraries.Utilities;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Drivers
{
    public class AzureApplicationInsightsTelemetrySettingsPartDriver : ContentPartDriver<AzureApplicationInsightsTelemetrySettingsPart>
    {
        private readonly IDeferredAppDomainRestarter _appDomainRestarter;


        public AzureApplicationInsightsTelemetrySettingsPartDriver(IDeferredAppDomainRestarter appDomainRestarter)
        {
            _appDomainRestarter = appDomainRestarter;
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
                        var previousEnableDependencyTracking = part.ApplicationWideDependencyTrackingIsEnabled;

                        updater.TryUpdateModel(part, Prefix, null, null);

                        if (previousEnableDependencyTracking != part.ApplicationWideDependencyTrackingIsEnabled)
                        {
                            _appDomainRestarter.RestartAppDomainWhenRequestEnds();
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