using System;
using System.Text;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Handlers;
using Orchard.Environment;
using Orchard.Localization;
using Orchard.Security;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Handlers
{
    public class AzureApplicationInsightsTelemetrySettingsPartHandler : ContentHandler
    {
        public Localizer T { get; set; }


        public AzureApplicationInsightsTelemetrySettingsPartHandler(Work<IEncryptionService> encryptionServiceWork)
        {
            T = NullLocalizer.Instance;

            Filters.Add(new ActivatingFilter<AzureApplicationInsightsTelemetrySettingsPart>("Site"));

            OnActivated<AzureApplicationInsightsTelemetrySettingsPart>((ctx, part) =>
            {
                part.ApiKeyField.Loader(() =>
                {
                    var encryptedApiKey = part.Retrieve<string>("EncryptedApiKey");

                    if (!string.IsNullOrEmpty(encryptedApiKey))
                    {
                        return Encoding.UTF8.GetString(encryptionServiceWork.Value.Decode(Convert.FromBase64String(encryptedApiKey)));
                    }

                    return string.Empty;
                });

                part.ApiKeyField.Setter(value =>
                {
                    if (value == null) value = string.Empty;

                    part.Store(
                        "EncryptedApiKey",
                        Convert.ToBase64String(encryptionServiceWork.Value.Encode(Encoding.UTF8.GetBytes(value))));

                    return value;
                });
            });
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