using Lombiq.Hosting.Azure.ApplicationInsights.Configuration;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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

            ////services.AddSingleton<ITelemetryModule, FileDiagnosticsTelemetryModule>();
            ////services.ConfigureTelemetryModule<FileDiagnosticsTelemetryModule>((module, options) => {
            ////    module.LogFilePath = @"D:\Projects\Clients\Finitive\SDKLOGS";
            ////    module.LogFileName = "mylog.txt";
            ////    module.Severity = "Verbose";
            ////});

            //services.AddLogging(loggingBuilder =>
            //{
            //    loggingBuilder.AddApplicationInsights();

            //    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Information);
            //    loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", LogLevel.Error);
            //});

            ////services.AddLogging(delegate (ILoggingBuilder loggingBuilder)
            ////{
            ////    loggingBuilder.AddApplicationInsights();
            ////    loggingBuilder.Services.Configure(delegate (LoggerFilterOptions options)
            ////    {
            ////        options.Rules.Insert(0, new LoggerFilterRule("Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider", null, LogLevel.Information, null));
            ////    });
            ////});
        }

        public override void Configure(IApplicationBuilder app, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            app.UseMiddleware<TestMiddleware>();

            var z = serviceProvider.GetServices<ILoggerProvider>();
            var logger = z.Last();
            serviceProvider.GetService<ILoggerFactory>().AddProvider(logger);

            var logLevelSection = serviceProvider.GetService<IConfiguration>().GetSection("Logging:ApplicationInsights:LogLevel");
            var logLevel = logLevelSection.GetValue<LogLevel>(string.Empty, LogLevel.None);
            var y = logLevelSection.GetValue<LogLevel>("Default", LogLevel.None);
            //serviceProvider.GetService<ILoggerFactory>().AddApplicationInsights(serviceProvider);
        }

        //public static ILoggerFactory AddApplicationInsights(
        //    this ILoggerFactory factory,
        //    IServiceProvider serviceProvider,
        //    Func<string, LogLevel, bool> filter,
        //    Action loggerAddedCallback)
        //{
        //    if (factory == null)
        //    {
        //        throw new ArgumentNullException(nameof(factory));
        //    }

        //    var client = serviceProvider.GetService<TelemetryClient>();
        //    var debugLoggerControl = serviceProvider.GetService<ApplicationInsightsLoggerEvents>();
        //    var options = serviceProvider.GetService<IOptions<ApplicationInsightsLoggerOptions>>();

        //    if (options == null)
        //    {
        //        options = Options.Create(new ApplicationInsightsLoggerOptions());
        //    }

        //    if (debugLoggerControl != null)
        //    {
        //        debugLoggerControl.OnLoggerAdded();

        //        if (loggerAddedCallback != null)
        //        {
        //            debugLoggerControl.LoggerAdded += loggerAddedCallback;
        //        }
        //    }

        //    factory.AddProvider(new ApplicationInsightsLoggerProvider(client, filter, options));
        //    return factory;
        //}
    }
}
