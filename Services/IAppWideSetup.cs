using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    public interface IAppWideSetup : IDependency
    {
        void SetupAppWideServices(IAppWideServicesConfiguration configuration);
    }


    public interface IAppWideServicesConfiguration
    {
        TelemetryConfiguration TelemetryConfiguration { get;}
        string ApiKey { get; }
        bool EnableDependencyTracking { get; }
        bool EnableLogCollection { get; }
        bool EnableDebugSnapshotCollection { get; }
    }


    public class AppWideServicesConfiguration : IAppWideServicesConfiguration
    {
        public TelemetryConfiguration TelemetryConfiguration { get; set; }
        public string ApiKey { get; set; }
        public bool EnableDependencyTracking { get; set; }
        public bool EnableLogCollection { get; set; }
        public bool EnableDebugSnapshotCollection { get; set; }
    }
}
