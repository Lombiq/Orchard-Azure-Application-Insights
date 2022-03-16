using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Security.Claims;

namespace Lombiq.Hosting.Azure.ApplicationInsights.TelemetryInitializers;

internal class UserContextPopulatingTelemetryInitializer : ITelemetryInitializer
{
    private readonly IServiceProvider _serviceProvider;

    public UserContextPopulatingTelemetryInitializer(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    public void Initialize(ITelemetry telemetry)
    {
        var httpContext = _serviceProvider.GetHttpContextSafely();

        if (httpContext == null || telemetry is not RequestTelemetry requestTelemetry) return;

        var options = _serviceProvider.GetRequiredService<IOptions<ApplicationInsightsOptions>>().Value;

        var user = httpContext.User;
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!string.IsNullOrEmpty(userId))
        {
            requestTelemetry.TryAddProperty("UserId", userId);
        }

        if (options.EnableUserNameCollection && !string.IsNullOrEmpty(user?.Identity?.Name))
        {
            requestTelemetry.TryAddProperty("UserName", user.Identity.Name);
        }

        if (options.EnableUserAgentCollection && string.IsNullOrEmpty(requestTelemetry.Context.User.UserAgent))
        {
            // While there is requestTelemetry.Context.User.UserAgent that's not displayed on the Azure Portal.
            requestTelemetry.TryAddProperty("UserAgent", httpContext.Request.Headers["User-Agent"]);
        }

        if (options.EnableIpAddressCollection)
        {
            requestTelemetry.TryAddProperty("ClientIP", telemetry.Context.Location.Ip);
        }
    }
}
