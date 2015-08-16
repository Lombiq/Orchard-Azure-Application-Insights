using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard.Events;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Events
{
    /// <summary>
    /// Telemetry module initialization events.
    /// </summary>
    public interface ITelemetryModulesInitializationEventHandler : IEventHandler
    {
        /// <summary>
        /// Fires before telemetry modules are initialized.
        /// </summary>
        /// <param name="modules">
        /// The telemetry modules currently registered to be initialized. You can add or remove modules.
        /// </param>
        void TelemetryModulesInitializing(IList<ITelemetryModule> modules);
    }
}
