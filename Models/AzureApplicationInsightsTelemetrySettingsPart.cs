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

        public bool ApplicationWideDependencyTrackingIsEnabled
        {
            get { return this.Retrieve(x => x.ApplicationWideDependencyTrackingIsEnabled); }
            set { this.Store(x => x.ApplicationWideDependencyTrackingIsEnabled, value); }
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

        public bool BackgroundTaskTrackingIsEnabled
        {
            get { return this.Retrieve(x => x.BackgroundTaskTrackingIsEnabled); }
            set { this.Store(x => x.BackgroundTaskTrackingIsEnabled, value); }
        }


        public string Stringify()
        {
            return InstrumentationKey +
                ApplicationWideLogCollectionIsEnabled +
                ApplicationWideDependencyTrackingIsEnabled +
                RequestTrackingIsEnabled +
                ClientSideTrackingIsEnabled +
                BackgroundTaskTrackingIsEnabled;
        }
    }
}