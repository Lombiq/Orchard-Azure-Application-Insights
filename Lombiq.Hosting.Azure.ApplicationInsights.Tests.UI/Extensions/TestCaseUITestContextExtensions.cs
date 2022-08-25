using Lombiq.Tests.UI.Extensions;
using Lombiq.Tests.UI.Services;
using OpenQA.Selenium;
using Shouldly;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI.Extensions;

public static class TestCaseUITestContextExtensions
{
    /// <summary>
    /// Tests Application Insights tracking during offline operation. Use <see
    /// cref="OrchardCoreUITestExecutorConfigurationExtensions.EnableApplicationInsightsOfflineOperation"/> to switch
    /// the module into offline mode.
    /// </summary>
    public static void TestApplicationInsightsTrackingInOfflineOperation(this UITestContext context)
    {
        if (!OrchardCoreUITestExecutorConfigurationExtensions.IsOfflineOperationEnabled)
        {
            throw new InvalidOperationException(
                "Testing Application Insights tracking during offline operation only works if you switch the module " +
                "into offline mode by calling " +
                "OrchardCoreUITestExecutorConfigurationExtensions.EnableApplicationInsightsOfflineOperation before " +
                "the app starts.");
        }

        var appInsightsExist = context.ExecuteScript("return window.appInsights === 'enabled'") as bool?;

        // Our custom message helps debugging, otherwise from the test output you could only tell that a value should be
        // true but is false which is less than helpful.
        appInsightsExist.ShouldBe(expected: true, "The Application Insights module is not working or is not in offline mode.");
    }
}
