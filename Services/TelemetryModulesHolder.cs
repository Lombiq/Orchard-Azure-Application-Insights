using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights.Extensibility;
using Orchard;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Stores telemetry modules that should be application-wide singletons.
    /// </summary>
    public interface ITelemetryModulesHolder : ISingletonDependency
    {
        IEnumerable<ITelemetryModule> GetRegisteredModules();
        void RegisterTelemetryModule(ITelemetryModule module);
        void UnRegisterTelemetryModule(ITelemetryModule module);
        void Clear();
    }


    public class TelemetryModulesHolder : ITelemetryModulesHolder
    {
        private readonly object _lock = new object();
        private List<ITelemetryModule> _modules = new List<ITelemetryModule>();


        public IEnumerable<ITelemetryModule> GetRegisteredModules()
        {
            lock (_lock)
            {
                return _modules.ToArray();
            }
        }

        public void RegisterTelemetryModule(ITelemetryModule module)
        {
            lock (_lock)
            {
                _modules.Add(module);
            }
        }

        public void UnRegisterTelemetryModule(ITelemetryModule module)
        {
            lock (_lock)
            {
                _modules.Remove(module);

                DisposeModule(module);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                foreach (var module in _modules)
                {
                    DisposeModule(module); 
                }

                _modules.Clear();
            }
        }


        private void DisposeModule(ITelemetryModule module)
        {
            var disposeMethod = module.GetType().GetMethod("Dispose");
            if (disposeMethod != null)
            {
                disposeMethod.Invoke(module, null);
            }
        }
    }
}