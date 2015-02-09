﻿using System;
using System.Reflection;
using Orchard.Environment;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Redirects assembly resolutions for AI-related assemblies. This is instead of putting assembly redirects into the Web.config.
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
            /*
               Curently the following redirects are mimicked here (edit these when necessary after updating AI libraries):
                <dependentAssembly>
                  <assemblyIdentity name="Microsoft.Diagnostics.Tracing.EventSource" publicKeyToken="b03f5f7f11d50a3a" culture="neutral" />
                  <bindingRedirect oldVersion="0.0.0.0-1.1.14.0" newVersion="1.1.14.0" />
                </dependentAssembly>
                <dependentAssembly>
                  <assemblyIdentity name="Microsoft.ApplicationInsights" publicKeyToken="31bf3856ad364e35" culture="neutral" />
                  <bindingRedirect oldVersion="0.0.0.0-0.13.0.22715" newVersion="0.13.0.22715" />
                </dependentAssembly>
                <dependentAssembly>
                    <assemblyIdentity name="Microsoft.Diagnostics.Instrumentation.Extensions.Intercept" publicKeyToken="31bf3856ad364e35" culture="neutral" />
                    <bindingRedirect oldVersion="0.0.0.0-0.13.0.22715" newVersion="0.13.0.22715" />
                </dependentAssembly>
             */

            var assemblyShortName = args.Name.Split(',')[0];

            switch (assemblyShortName)
            {
                case "Microsoft.Diagnostics.Tracing.EventSource":
                    return typeof(Microsoft.Diagnostics.Tracing.EventAttribute).Assembly;
                case "Microsoft.ApplicationInsights":
                    return typeof(Microsoft.ApplicationInsights.TelemetryClient).Assembly;
                // For some reason this crashed IIS Express...
                //case "Microsoft.Diagnostics.Instrumentation.Extensions.Intercept":
                //    return typeof(Microsoft.Diagnostics.Instrumentation.Extensions.Intercept.Decorator).Assembly;
                default:
                    return null;
            }
        }
    }
}