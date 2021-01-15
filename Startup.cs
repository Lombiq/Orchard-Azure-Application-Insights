using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.ResourceManagement;
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
            // an injected IOptions<T> here but need to directly bind to ApplicationInsightsOptions.
            var options = new ApplicationInsightsOptions();
            var configSection = _shellConfiguration.GetSection("Lombiq_Hosting_Azure_ApplicationInsights");
            configSection.Bind(options);
            services.Configure<ApplicationInsightsOptions>(configSection);

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
                (module, serviceOptions) => module.EnableSqlCommandTextInstrumentation = options.EnableSqlCommandTextInstrumentation);

            services.AddScoped<IResourceManifestProvider, ResourceManifest>();

            services.Configure<MvcOptions>((options) => options.Filters.Add(typeof(TrackingScriptInjectingFilter)));
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            if (serviceProvider.GetService<IOptions<ApplicationInsightsOptions>>().Value.EnableLoggingTestMiddleware)
            {
                app.UseMiddleware<LoggingTestMiddleware>();
            }

            // For some reason the AI logger provider needs to be re-registered here otherwise no logging will happen.
            var aiProvider = serviceProvider.GetServices<ILoggerProvider>().Single(provider => provider is ApplicationInsightsLoggerProvider);
            serviceProvider.GetService<ILoggerFactory>().AddProvider(aiProvider);
            // There seems to be no way to apply a default filtering to this from code. Going via services.AddLogging()
            // in ConfigureServices() doesn't work, neither there. The rules get saved but are never applied. The
            // default
            //// { ProviderName: 'Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider', CategoryName: '', LogLevel: 'Warning', Filter: ''}
            // rule added by AddApplicationInsightsTelemetry() is there too but it doesn't take any effect. So, there's
            // no other option than add default configuration in appsettings or similar.
        }
    }
}
