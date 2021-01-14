using Lombiq.Hosting.Azure.ApplicationInsights.Configuration;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

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
    }
}
