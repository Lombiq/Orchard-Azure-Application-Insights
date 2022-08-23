using Lombiq.Tests.UI.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI.Extensions;

public static class OrchardCoreUITestExecutorConfigurationExtensions
{
    /// <summary>
    /// Adds a command line argument to the app during <see cref="OrchardCoreConfiguration.BeforeAppStart"/> that
    /// switches the Application Insights module into offline mode. This way it won't try to reach out to a remote
    /// server with telemetry and the test remains self-contained.
    /// </summary>
    public static void EnableApplicationInsightsOfflineOperation(this OrchardCoreUITestExecutorConfiguration configuration) =>
        configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .Add("--OrchardCore:Lombiq_Hosting_Azure_ApplicationInsights:EnableOfflineOperation")
                    .Add("true");

                return Task.CompletedTask;
            };
}
