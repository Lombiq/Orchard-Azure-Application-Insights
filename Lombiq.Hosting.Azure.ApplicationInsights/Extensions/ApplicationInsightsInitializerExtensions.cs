using Azure.Identity;
using Lombiq.Hosting.Azure.ApplicationInsights;
using Lombiq.Hosting.Azure.ApplicationInsights.Models;
using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector.QuickPulse;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Linq;
using ApplicationInsightsFeatureIds = Lombiq.Hosting.Azure.ApplicationInsights.Constants.FeatureIds;

namespace Microsoft.Extensions.DependencyInjection;

public static class ApplicationInsightsInitializerExtensions
{
    /// <summary>
    /// Recommended default configuration for features of an Orchard Core application hosted in Azure, with Application
    /// Insights telemetry. If any of the configuration values exist, they won't be overridden, so e.g.
    /// appsettings.json configuration will take precedence.
    /// </summary>
    /// <param name="webApplicationBuilder">The <see cref="WebApplicationBuilder"/> instance of the app.</param>
    /// <param name="hostingConfiguration">Configuration for the hosting defaults.</param>
    public static OrchardCoreBuilder ConfigureAzureHostingDefaultsWithApplicationInsightsTelemetry(
        this OrchardCoreBuilder builder,
        WebApplicationBuilder webApplicationBuilder,
        AzureHostingConfiguration hostingConfiguration = null)
    {
        builder
            .ConfigureAzureHostingDefaults(webApplicationBuilder, hostingConfiguration)
            .AddOrchardCoreApplicationInsightsTelemetry(webApplicationBuilder.Configuration);

        var logLevelSection = webApplicationBuilder.Configuration.GetSection("Logging:ApplicationInsights:LogLevel");

        logLevelSection.AddValueIfKeyNotExists("Default", "Warning");

        var ocAppInsightsSection = webApplicationBuilder.Configuration.GetSection("OrchardCore:Lombiq_Hosting_Azure_ApplicationInsights");

        ocAppInsightsSection
            .AddValueIfKeyNotExists("EnableUserNameCollection", "true")
            .AddValueIfKeyNotExists("EnableUserAgentCollection", "true")
            .AddValueIfKeyNotExists("EnableIpAddressCollection", "true");

        if (webApplicationBuilder.Environment.IsDevelopment())
        {
            ocAppInsightsSection.AddValueIfKeyNotExists("EnableLoggingTestMiddleware", "true");

            var appInsightsSection = webApplicationBuilder.Configuration.GetSection("ApplicationInsights");

            appInsightsSection.AddValueIfKeyNotExists("EnableDependencyTrackingTelemetryModule", "false");
        }

        return builder;
    }

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

        services.Configure<TelemetryConfiguration>(config =>
        {
            if (applicationInsightsOptions.EntraAuthenticationType == EntraAuthenticationType.ServicePrincipal)
            {
                var credential = new ClientSecretCredential(
                    applicationInsightsOptions.ServicePrincipalCredentials.TenantId,
                    applicationInsightsOptions.ServicePrincipalCredentials.ClientId,
                    applicationInsightsOptions.ServicePrincipalCredentials.ClientSecret);
                config.SetAzureTokenCredential(credential);
            }

            if (applicationInsightsOptions.EntraAuthenticationType == EntraAuthenticationType.ManagedIdentity)
            {
                var credential = new DefaultAzureCredential();
                config.SetAzureTokenCredential(credential);
            }
        });

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
        services.AddApplicationInsightsTelemetryProcessor<AzureBlobTelemetryFilter>();

        services.Configure<ApplicationInsightsOptions>(applicationInsightsConfigSection);

        services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>(
            (module, _) => module.EnableSqlCommandTextInstrumentation = applicationInsightsOptions.EnableSqlCommandTextInstrumentation);

#pragma warning disable CS0618 // Type or member is obsolete
        if (applicationInsightsOptions.EntraAuthenticationType == EntraAuthenticationType.None &&
            !string.IsNullOrEmpty(applicationInsightsOptions.QuickPulseTelemetryModuleAuthenticationApiKey))
        {
            services.ConfigureTelemetryModule<QuickPulseTelemetryModule>(
                (module, _) => module.AuthenticationApiKey = applicationInsightsOptions.QuickPulseTelemetryModuleAuthenticationApiKey);
        }
#pragma warning restore CS0618 // Type or member is obsolete

        services.AddSingleton<ITelemetryInitializer, UserContextPopulatingTelemetryInitializer>();
        services.AddSingleton<ITelemetryInitializer, ShellNamePopulatingTelemetryInitializer>();
        services.AddSingleton<ITelemetryInitializer, IgnoreFailureTelemetryInitializer>();
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
