using Orchard.ContentManagement;
using Orchard.ContentManagement.Utilities;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Models
{
    public class AzureApplicationInsightsTelemetrySettingsPart : ContentPart, ITelemetrySettings
    {
        public string InstrumentationKey
        {
            get { return this.Retrieve(x => x.InstrumentationKey); }
            set { this.Store(x => x.InstrumentationKey, value); }
        }

        private readonly LazyField<string> _apiKey = new LazyField<string>();
        internal LazyField<string> ApiKeyField => _apiKey;
        public string ApiKey
        {
            get { return _apiKey.Value; }
            set { _apiKey.Value = value; }
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

        public bool ApplicationWideDebugSnapshotCollectionIsEnabled
        {
            get { return this.Retrieve(x => x.ApplicationWideDebugSnapshotCollectionIsEnabled, true); }
            set { this.Store(x => x.ApplicationWideDebugSnapshotCollectionIsEnabled, value); }
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
            return 
                InstrumentationKey +
                ApiKey +
                ApplicationWideLogCollectionIsEnabled +
                ApplicationWideDependencyTrackingIsEnabled +
                ApplicationWideDebugSnapshotCollectionIsEnabled +
                RequestTrackingIsEnabled +
                ClientSideTrackingIsEnabled +
                BackgroundTaskTrackingIsEnabled;
        }
    }
}