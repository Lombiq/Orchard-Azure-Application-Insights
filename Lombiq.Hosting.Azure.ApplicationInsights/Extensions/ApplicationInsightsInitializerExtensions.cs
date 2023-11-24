using Lombiq.Hosting.Azure.ApplicationInsights;
using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
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
        IConfiguration configurationManager)
    {
        var services = builder.ApplicationServices;
        services.AddApplicationInsightsTelemetry(configurationManager);

        // Create a temporary ServiceProvider to configure ApplicationInsightsServiceOptions.
        using var serviceProvider = services.BuildServiceProvider();
        var applicationInsightsServiceOptions = serviceProvider
            .GetService<IOptions<ApplicationInsightsServiceOptions>>()?.Value;

        var applicationInsightsOptions = new ApplicationInsightsOptions();
        var applicationInsightsConfigSection = configurationManager
            .GetSection("OrchardCore:Lombiq_Hosting_Azure_ApplicationInsights");
        applicationInsightsConfigSection.Bind(applicationInsightsOptions);

        if (string.IsNullOrEmpty(applicationInsightsServiceOptions?.ConnectionString) &&
#pragma warning disable CS0618 // Type or member is obsolete
            string.IsNullOrEmpty(applicationInsightsServiceOptions?.InstrumentationKey) &&
#pragma warning restore CS0618 // Type or member is obsolete
            !applicationInsightsOptions.EnableOfflineOperation)
        {
            // Removing ITelemetryModules from the service collection is necessary because otherwise the modules will be
            // used, even if there is no ConnectionString or InstrumentationKey added.
            var descriptorsToDelete = services.Where(descriptor => descriptor.ServiceType == typeof(ITelemetryModule)).ToArray();

            foreach (var descriptor in descriptorsToDelete)
            {
                services.Remove(descriptor);
            }

            return builder;
        }

        services.AddApplicationInsightsTelemetryProcessor<TelemetryFilter>();

        services.Configure<ApplicationInsightsOptions>(applicationInsightsConfigSection);

        services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
            (module, _) => module.EnableSqlCommandTextInstrumentation = applicationInsightsOptions.EnableSqlCommandTextInstrumentation);

        services.ConfigureTelemetryModule<QuickPulseTelemetryModule>(
            (module, _) => module.AuthenticationApiKey = applicationInsightsOptions.QuickPulseTelemetryModuleAuthenticationApiKey);

        services.AddSingleton<ITelemetryInitializer, UserContextPopulatingTelemetryInitializer>();
        services.AddSingleton<ITelemetryInitializer, ShellNamePopulatingTelemetryInitializer>();
        services.AddSingleton<ITelemetryInitializer, IgnoreFailureInitializer>();
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

    /// <summary>
    /// Adds an Application Insights Telemetry Processor into the <c>ApplicationServices</c> service collection.
    /// </summary>
    public static OrchardCoreBuilder AddApplicationInsightsTelemetryProcessor<T>(this OrchardCoreBuilder builder)
        where T : ITelemetryProcessor
    {
        builder.ApplicationServices.AddApplicationInsightsTelemetryProcessor<T>();
        return builder;
    }
}
