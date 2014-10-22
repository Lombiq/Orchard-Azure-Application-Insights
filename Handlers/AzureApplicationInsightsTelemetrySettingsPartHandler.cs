using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Localization;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Handlers
{
    public class AzureApplicationInsightsTelemetrySettingsPartHandler : ContentHandler
    {
        public Localizer T { get; set; }


        public AzureApplicationInsightsTelemetrySettingsPartHandler()
        {
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<AzureApplicationInsightsTelemetrySettingsPart>("Site"));
        }


        protected override void GetItemMetadata(GetContentItemMetadataContext context)
        {
            if (context.ContentItem.ContentType != "Site")
                return;

            base.GetItemMetadata(context);

            context.Metadata.EditorGroupInfo.Add(new GroupInfo(T("Azure Application Insights")) { Id = Constants.SiteSettingsEditorGroup });
        }
    }
}