using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
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
            get { return this.Retrieve(x => x.ApplicationWideLogCollectionIsEnabled); }
            set { this.Store(x => x.ApplicationWideLogCollectionIsEnabled, value); }
        }
    }
}