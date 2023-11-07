using Lombiq.Hosting.Azure.ApplicationInsights;
using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Extensions.Configuration;
using System.Linq;
using ApplicationInsightsFeatureIds = Lombiq.Hosting.Azure.ApplicationInsights.Constants.FeatureIds;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationInsightsInitializerExtensions
{
    /// <summary>
    /// Initializes Application Insights for Orchard Core. Should be used in the application Program.cs file.
    /// </summary>
    public static OrchardCoreBuilder AddOrchardCoreApplicationInsightsTelemetry(
        this OrchardCoreBuilder builder,
        IServiceCollection services,
        IConfiguration configurationManager)
    {
        var applicationInsightsServiceOptions = new ApplicationInsightsServiceOptions();
        var applicationInsightsServiceConfigSection = configurationManager.GetSection("ApplicationInsights");
        applicationInsightsServiceConfigSection.Bind(applicationInsightsServiceOptions);

        // Since the below AI configuration needs to happen during app startup in ConfigureServices() we can't use an
        // injected IOptions<T> here but need to directly bind to ApplicationInsightsOptions.
        var applicationInsightsOptions = new ApplicationInsightsOptions();
        var applicationInsightsConfigSection = configurationManager.GetSection("OrchardCore:Lombiq_Hosting_Azure_ApplicationInsights");
        applicationInsightsConfigSection.Bind(applicationInsightsOptions);

        if (string.IsNullOrEmpty(applicationInsightsServiceOptions.ConnectionString) &&
#pragma warning disable CS0618 // Type or member is obsolete
            string.IsNullOrEmpty(applicationInsightsServiceOptions.InstrumentationKey) &&
#pragma warning restore CS0618 // Type or member is obsolete
            !applicationInsightsOptions.EnableOfflineOperation)
        {
            return builder;
        }

        services.AddApplicationInsightsTelemetry(configurationManager);
        services.AddApplicationInsightsTelemetryProcessor<TelemetryFilter>();

        services.Configure<ApplicationInsightsOptions>(applicationInsightsConfigSection);

        services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
            (module, _) => module.EnableSqlCommandTextInstrumentation = applicationInsightsOptions.EnableSqlCommandTextInstrumentation);

        services.ConfigureTelemetryModule<QuickPulseTelemetryModule>(
            (module, _) => module.AuthenticationApiKey = applicationInsightsOptions.QuickPulseTelemetryModuleAuthenticationApiKey);

        services.AddSingleton<ITelemetryInitializer, UserContextPopulatingTelemetryInitializer>();
        services.AddSingleton<ITelemetryInitializer, ShellNamePopulatingTelemetryInitializer>();
        services.AddScoped<ITrackingScriptFactory, TrackingScriptFactory>();

        if (applicationInsightsOptions.EnableOfflineOperation)
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

        builder.AddTenantFeatures(ApplicationInsightsFeatureIds.Default);

        return builder;
    }
}
