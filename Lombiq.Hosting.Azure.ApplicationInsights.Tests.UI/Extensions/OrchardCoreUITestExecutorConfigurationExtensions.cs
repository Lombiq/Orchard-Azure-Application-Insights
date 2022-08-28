using Lombiq.Tests.UI.Services;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI.Extensions;

public static class OrchardCoreUITestExecutorConfigurationExtensions
{
    internal static bool IsOfflineOperationEnabled { get; private set; }

    /// <summary>
    /// Adds a command line argument to the app during <see cref="OrchardCoreConfiguration.BeforeAppStart"/> that
    /// switches the Application Insights module into offline mode. This way it won't try to reach out to a remote
    /// server with telemetry and the test remains self-contained.
    /// </summary>
    public static void EnableApplicationInsightsOfflineOperation(this OrchardCoreUITestExecutorConfiguration configuration)
    {
        IsOfflineOperationEnabled = true;

        configuration.OrchardCoreConfiguration.BeforeAppStart +=
            (_, argumentsBuilder) =>
            {
                argumentsBuilder
                    .AddValue("OrchardCore:Lombiq_Hosting_Azure_ApplicationInsights:EnableOfflineOperation", "true");

                return Task.CompletedTask;
            };
    }
}
