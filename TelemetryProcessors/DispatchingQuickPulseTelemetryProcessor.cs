using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryProcessors
{
    internal class DispatchingQuickPulseTelemetryProcessor : QuickPulseTelemetryProcessor, ITelemetryProcessor
    {
        private readonly QuickPulseTelemetryProcessor _appWideQuickPulseTelemetryProcessor;


        public DispatchingQuickPulseTelemetryProcessor(
            ITelemetryProcessor next, 
            QuickPulseTelemetryProcessor appWideQuickPulseTelemetryProcessor) : base(next)
        {
            _appWideQuickPulseTelemetryProcessor = appWideQuickPulseTelemetryProcessor;
        }


        void ITelemetryProcessor.Process(ITelemetry item)
        {
            Process(item);
            _appWideQuickPulseTelemetryProcessor.Process(item);
        }
    }
}