using System.Web.Mvc;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Orchard;
using Orchard.ContentManagement;
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

            var workContext = _workContextAccessor.GetContext();

            if (!workContext.CurrentSite.As<AzureApplicationInsightsTelemetrySettingsPart>().ClientSideTrackingIsEnabled) return;

            var instrumentationKey = _telemetrySettingsAccessor.GetCurrentSettings().InstrumentationKey;

            if (string.IsNullOrEmpty(instrumentationKey)) return;

            workContext.Layout.Head
                .Add(_shapeFactory.Azure_ApplicationInsights_TrackingScript(InstrumentationKey: instrumentationKey));
        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }
    }
}