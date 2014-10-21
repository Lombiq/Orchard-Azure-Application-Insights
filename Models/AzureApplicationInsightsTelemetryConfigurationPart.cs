using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Orchard.ContentManagement;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Models
{
    public class AzureApplicationInsightsTelemetryConfigurationPart : ContentPart
    {
        [Required]
        public string InstrumentationKey
        {
            get { return this.Retrieve(x => x.InstrumentationKey); }
            set { this.Store(x => x.InstrumentationKey, value); }
        }

        public bool EnableLogCollection
        {
            get { return this.Retrieve(x => x.EnableLogCollection); }
            set { this.Store(x => x.EnableLogCollection, value); }
        }
    }
}