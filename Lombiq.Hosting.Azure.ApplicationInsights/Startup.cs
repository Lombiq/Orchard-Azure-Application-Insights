using Lombiq.Hosting.Azure.ApplicationInsights.Services;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights;

public class Startup(
    IOptions<ApplicationInsightsOptions> applicationInsightsOptions,
    IOptions<ApplicationInsightsServiceOptions> applicationInsightsServiceOptions) : StartupBase
{
    private readonly ApplicationInsightsOptions _applicationInsightsOptions = applicationInsightsOptions.Value;
    private readonly ApplicationInsightsServiceOptions _applicationInsightsServiceOptions = applicationInsightsServiceOptions.Value;

    public override void ConfigureServices(IServiceCollection services)
    {
        if (string.IsNullOrEmpty(_applicationInsightsServiceOptions.ConnectionString) &&
#pragma warning disable CS0618 // Type or member is obsolete
            string.IsNullOrEmpty(_applicationInsightsServiceOptions.InstrumentationKey) &&
#pragma warning restore CS0618 // Type or member is obsolete
            !_applicationInsightsOptions.EnableOfflineOperation)
        {
            return;
        }

        services.Configure<MvcOptions>((options) => options.Filters.Add(typeof(TrackingScriptInjectingFilter)));

        if (_applicationInsightsOptions.EnableLoggingTestBackgroundTask)
        {
            services.AddSingleton<IBackgroundTask, LoggingTestBackgroundTask>();
        }

        if (_applicationInsightsOptions.EnableBackgroundTaskTelemetryCollection)
        {
            services.AddScoped<IBackgroundTaskEventHandler, BackgroundTaskTelemetryEventHandler>();
        }
    }

    public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<ApplicationInsightsOptions>>().Value;

        if (options.EnableLoggingTestMiddleware) app.UseMiddleware<LoggingTestMiddleware>();
        // There seems to be no way to apply a default filtering to this from code. Going via services.AddLogging() in
        // ConfigureServices() doesn't work, neither there. The rules get saved but are never applied. The default
        ////{
        ////    "ProviderName": "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider",
        ////    "CategoryName": "",
        ////    "LogLevel": "Warning",
        ////    "Filter": ""
        ////}
        // rule added by AddApplicationInsightsTelemetry() is there too but it doesn't take any effect. So, there's no
        // other option than add default configuration in appsettings or similar.
    }
}
