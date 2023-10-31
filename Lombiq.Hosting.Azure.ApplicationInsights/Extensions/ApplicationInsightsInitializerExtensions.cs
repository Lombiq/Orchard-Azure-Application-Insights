using Lombiq.Hosting.Azure.ApplicationInsights;
using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationInsightsInitializerExtensions
{
    /// <summary>
    /// Use this extension method to initialize Application Insights. Should be used in the application Program.cs file.
    /// </summary>
    public static IServiceCollection AddOrchardCoreApplicationInsightsTelemetry(
        this IServiceCollection services,
        ConfigurationManager configurationManager)
    {
        services.AddApplicationInsightsTelemetry(configurationManager);
        services.AddApplicationInsightsTelemetryProcessor<TelemetryFilter>();

        // Since the below AI configuration needs to happen during app startup in ConfigureServices() we can't use an
        // injected IOptions<T> here but need to directly bind to ApplicationInsightsOptions.
        var options = new ApplicationInsightsOptions();
        var configSection = configurationManager.GetSection("OrchardCore:Lombiq_Hosting_Azure_ApplicationInsights");
        configSection.Bind(options);
        services.Configure<ApplicationInsightsOptions>(configSection);

        services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
            (module, _) => module.EnableSqlCommandTextInstrumentation = options.EnableSqlCommandTextInstrumentation);

        services.ConfigureTelemetryModule<QuickPulseTelemetryModule>(
            (module, _) => module.AuthenticationApiKey = options.QuickPulseTelemetryModuleAuthenticationApiKey);

        services.AddSingleton<ITelemetryInitializer, UserContextPopulatingTelemetryInitializer>();
        services.AddSingleton<ITelemetryInitializer, ShellNamePopulatingTelemetryInitializer>();
        services.Configure<MvcOptions>((options) => options.Filters.Add(typeof(TrackingScriptInjectingFilter)));
        services.AddScoped<ITrackingScriptFactory, TrackingScriptFactory>();

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

        return services;
    }
}
