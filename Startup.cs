using Lombiq.Hosting.Azure.ApplicationInsights.Configuration;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using System;
using System.Linq;

namespace Lombiq.Hosting.Azure.ApplicationInsights
{
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;

        public Startup(IShellConfiguration shellConfiguration) =>
            _shellConfiguration = shellConfiguration;

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationInsightsTelemetry(_shellConfiguration);

            // Since the below AI configuration needs to happen during app startup in ConfigureServices() we can't use
            // an injected IOptions<T> but need to directly bind to ApplicationInsightsOptions.
            var options = new ApplicationInsightsOptions();
            _shellConfiguration.GetSection("Lombiq_Hosting_Azure_ApplicationInsights").Bind(options);

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
                (module, serviceOptions) => module.EnableSqlCommandTextInstrumentation = options.EnableSqlCommandTextInstrumentation);
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseMiddleware<TestMiddleware>();

            // For some reason the AI logger provider needs to be re-registered here otherwise no logging will happen.
            var aiProvider = serviceProvider.GetServices<ILoggerProvider>().Single(provider => provider is ApplicationInsightsLoggerProvider);
            serviceProvider.GetService<ILoggerFactory>().AddProvider(aiProvider);
        }
    }
}
