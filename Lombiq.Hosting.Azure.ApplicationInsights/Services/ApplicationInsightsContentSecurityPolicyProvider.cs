using Lombiq.HelpfulLibraries.AspNetCore.Security;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

using static Lombiq.HelpfulLibraries.AspNetCore.Security.ContentSecurityPolicyDirectives;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

internal sealed class ApplicationInsightsContentSecurityPolicyProvider : IContentSecurityPolicyProvider
{
    public ValueTask UpdateAsync(IDictionary<string, string> securityPolicies, HttpContext context)
    {
        CspHelper.MergeValues(securityPolicies, ScriptSrc, "js.monitor.azure.com");
        CspHelper.MergeValues(securityPolicies, ConnectSrc, "*.applicationinsights.azure.com");

        return ValueTask.CompletedTask;
    }
}
