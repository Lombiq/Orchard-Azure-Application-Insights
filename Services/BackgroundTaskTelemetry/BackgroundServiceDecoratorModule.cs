using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Tasks;
using Piedone.HelpfulLibraries.Libraries.DependencyInjection;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services.BackgroundTaskTelemetry
{
    public class BackgroundServiceDecoratorModule : DecoratorsModuleBase
    {
        protected override IEnumerable<DecorationConfiguration> DescribeDecorators()
        {
            return new[]
            {
                DecorationConfiguration.Create<IBackgroundService, BackgroundServiceDecorator>()
            };
        }
    }
}