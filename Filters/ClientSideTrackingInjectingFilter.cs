using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Orchard;
using Orchard.DisplayManagement;
using Orchard.Mvc.Filters;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Filters
{
    public class ClientSideTrackingInjectingFilter : FilterProvider, IResultFilter
    {
        private readonly IWorkContextAccessor _workContextAccessor;
        private readonly dynamic _shapeFactory;
        private readonly ITelemetrySettingsAccessor _telemetrySettingsAccessor;


        public ClientSideTrackingInjectingFilter(
            IWorkContextAccessor workContextAccessor,
            IShapeFactory shapeFactory,
            ITelemetrySettingsAccessor telemetrySettingsAccessor)
        {
            _workContextAccessor = workContextAccessor;
            _shapeFactory = shapeFactory;
            _telemetrySettingsAccessor = telemetrySettingsAccessor;
        }


        public void OnResultExecuting(ResultExecutingContext filterContext)
        {
            // Should only run on a full view rendering result.
            if (!(filterContext.Result is ViewResult))
                return;

            var instrumentationKey = _telemetrySettingsAccessor.GetDefaultSettings().InstrumentationKey;

            if (string.IsNullOrEmpty(instrumentationKey)) return;

            _workContextAccessor.GetContext().Layout.Head
                .Add(_shapeFactory.Azure_ApplicationInsights_TrackingScript(InstrumentationKey: instrumentationKey));
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}