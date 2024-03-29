using Lombiq.Privacy.Tests.UI.Extensions;
using Lombiq.Tests.UI;
using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using Shouldly;
using System;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI.TestCases;

public static class ApplicationInsightsTestCases
{
    public static Task ApplicationInsightsTrackingInOfflineOperationShouldWorkAsync(
        ExecuteTestAfterSetupAsync executeTestAfterSetupAsync,
        Browser browser = default,
        Func<OrchardCoreUITestExecutorConfiguration, Task> changeConfigurationAsync = null) =>
        executeTestAfterSetupAsync(
            async context =>
            {
                await context.EnablePrivacyConsentBannerFeatureAndAcceptPrivacyConsentAsync();

                var appInsightsExist = context.ExecuteScript("return window.appInsights === 'enabled'") as bool?;

                // Our custom message helps debugging, otherwise from the test output you could only tell that a value
                // should be true but is false which is less than helpful.
                appInsightsExist.ShouldBe(expected: true, "The Application Insights module is not working or is not in offline mode.");
            },
            browser,
            async configuration =>
            {
                if (changeConfigurationAsync != null) await changeConfigurationAsync(configuration);

                configuration.OrchardCoreConfiguration.BeforeAppStart +=
                    (_, argumentsBuilder) =>
                    {
                        argumentsBuilder
                            .AddWithValue(
                                "OrchardCore:Lombiq_Hosting_Azure_ApplicationInsights:EnableOfflineOperation",
                                value: true);

                        return Task.CompletedTask;
                    };
            });
}
