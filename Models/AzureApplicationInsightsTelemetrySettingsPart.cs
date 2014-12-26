using Orchard.ContentManagement;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Models
{
    public class AzureApplicationInsightsTelemetrySettingsPart : ContentPart, ITelemetrySettings
    {
        public string InstrumentationKey
        {
            get { return this.Retrieve(x => x.InstrumentationKey); }
            set { this.Store(x => x.InstrumentationKey, value); }
        }

        public bool ApplicationWideLogCollectionIsEnabled
        {
            get { return this.Retrieve(x => x.ApplicationWideLogCollectionIsEnabled, true); }
            set { this.Store(x => x.ApplicationWideLogCollectionIsEnabled, value); }
        }

        public bool RequestTrackingIsEnabled
        {
            get { return this.Retrieve(x => x.RequestTrackingIsEnabled, true); }
            set { this.Store(x => x.RequestTrackingIsEnabled, value); }
        }

        public bool ClientSideTrackingIsEnabled
        {
            get { return this.Retrieve(x => x.ClientSideTrackingIsEnabled, true); }
            set { this.Store(x => x.ClientSideTrackingIsEnabled, value); }
        }
    }
}