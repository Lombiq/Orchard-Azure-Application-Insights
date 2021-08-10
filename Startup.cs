using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
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
            // an injected IOptions<T> here but need to directly bind to ApplicationInsightsOptions.
            var options = new ApplicationInsightsOptions();
            var configSection = _shellConfiguration.GetSection("Lombiq_Hosting_Azure_ApplicationInsights");
            configSection.Bind(options);
            services.Configure<ApplicationInsightsOptions>(configSection);

            services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
                (module, serviceOptions) => module.EnableSqlCommandTextInstrumentation = options.EnableSqlCommandTextInstrumentation);

            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>(
                (module, serviceOptions) => module.AuthenticationApiKey = options.QuickPulseTelemetryModuleAuthenticationApiKey);

            services.AddSingleton<ITelemetryInitializer, UserContextPopulatingTelemetryInitializer>();
            services.AddSingleton<ITelemetryInitializer, ShellNamePopulatingTelemetryInitializer>();
            services.Configure<MvcOptions>((options) => options.Filters.Add(typeof(TrackingScriptInjectingFilter)));
            services.AddScoped<ITrackingScriptFactory, TrackingScriptFactory>();

            if (options.EnableLoggingTestBackgroundTask)
            {
                services.AddSingleton<IBackgroundTask, LoggingTestBackgroundTask>();
            }

            if (options.EnableBackgroundTaskTelemetryCollection)
            {
                services.AddScoped<IBackgroundTaskEventHandler, BackgroundTaskTelemetryEventHandler>();
            }

            if (options.EnableOfflineOperation)
            {
                foreach (var descriptor in services.Where(descriptor => descriptor.ServiceType == typeof(ITelemetryChannel)).ToArray())
                {
                    services.Remove(descriptor);
                }

                services.AddSingleton<ITelemetryChannel, NullTelemetryChannel>();
                services.Configure<ApplicationInsightsServiceOptions>(
                    options =>
                    {
                        options.EnableAppServicesHeartbeatTelemetryModule = false;
                        options.EnableHeartbeat = false;
                        options.EnableQuickPulseMetricStream = false;
                    });
            }
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var options = serviceProvider.GetService<IOptions<ApplicationInsightsOptions>>().Value;

            if (options.EnableLoggingTestMiddleware) app.UseMiddleware<LoggingTestMiddleware>();

            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();

            // For some reason the AI logger provider needs to be re-registered here otherwise no logging will happen.
            var aiProvider = serviceProvider.GetServices<ILoggerProvider>().Single(provider => provider is ApplicationInsightsLoggerProvider);
            loggerFactory.AddProvider(aiProvider);
            // There seems to be no way to apply a default filtering to this from code. Going via services.AddLogging()
            // in ConfigureServices() doesn't work, neither there. The rules get saved but are never applied. The
            // default
            ////{
            ////    "ProviderName": "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider",
            ////    "CategoryName": "",
            ////    "LogLevel": "Warning",
            ////    "Filter": ""
            ////}
            // rule added by AddApplicationInsightsTelemetry() is there too but it doesn't take any effect. So, there's
            // no other option than add default configuration in appsettings or similar.
        }
    }
}
