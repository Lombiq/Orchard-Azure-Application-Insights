using System;
using System.Reflection;
using Orchard.Environment;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Redirects assembly resolutions for AI-related assemblies. This is instead of putting assembly redirects into the 
    /// Web.config.
    /// </summary>
    public class AssemblyRedirectSetupShellEventHandler : IOrchardShellEvents
    {
        public void Activated()
        {
            // Trying to remove first so no duplicate event registration can occur.
            AppDomain.CurrentDomain.AssemblyResolve -= ResolveApplicationInsightsAssemblies;
            AppDomain.CurrentDomain.AssemblyResolve += ResolveApplicationInsightsAssemblies;
        }

        public void Terminating()
        {
        }


        private Assembly ResolveApplicationInsightsAssemblies(object sender, ResolveEventArgs args)
        {
            // These are here instead of adding assembly redirects to the Web.config. Edit them when necessary after 
            // updating AI libraries

            var assemblyShortName = args.Name.Split(',')[0];

            switch (assemblyShortName)
            {
                case "Microsoft.ApplicationInsights":
                    return typeof(Microsoft.ApplicationInsights.TelemetryClient).Assembly;
                case "Microsoft.Diagnostics.Instrumentation.Extensions.Intercept":
                    return typeof(Microsoft.Diagnostics.Instrumentation.Extensions.Intercept.Decorator).Assembly;
                case "Microsoft.AI.Agent.Intercept":
                    return typeof(Microsoft.Diagnostics.Instrumentation.Extensions.Base.Callbacks).Assembly;
                case "log4net":
                    return Assembly.Load("log4net");
                default:
                    return null;
            }
        }
    }
}