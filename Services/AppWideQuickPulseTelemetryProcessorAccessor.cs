using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Orchard;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    /// <summary>
    /// Service for accessing the single app-wide QuickPulseTelemetryProcessor necessary for using the AI Live Metrics 
    /// Stream feature.
    /// </summary>
    /// <remarks>
    /// See: https://github.com/Lombiq/Orchard-Azure-Application-Insights/issues/6
    /// </remarks>
    public interface IAppWideQuickPulseTelemetryProcessorAccessor : IDependency
    {
        QuickPulseTelemetryProcessor GetAppWideQuickPulseTelemetryProcessor();
    }


    public class AppWideQuickPulseTelemetryProcessorAccessor : IAppWideQuickPulseTelemetryProcessorAccessor
    {
        private static QuickPulseTelemetryProcessor _quickPulseTelemetryProcessor =
            new QuickPulseTelemetryProcessor(new NullTelemetryProcessor());


        public QuickPulseTelemetryProcessor GetAppWideQuickPulseTelemetryProcessor()
        {
            return _quickPulseTelemetryProcessor;
        }


        private class NullTelemetryProcessor : ITelemetryProcessor
        {
            public void Process(ITelemetry item)
            {
            }
        }
    }
}