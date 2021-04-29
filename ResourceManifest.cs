using Lombiq.Hosting.Azure.ApplicationInsights.Constants;
using Microsoft.ApplicationInsights.AspNetCore;
using Microsoft.Extensions.Options;
using OrchardCore.ResourceManagement;

namespace Lombiq.Hosting.Azure.ApplicationInsights
{
    public class ResourceManagementOptionsConfiguration : IConfigureOptions<ResourceManagementOptions>
    {
        private readonly JavaScriptSnippet _javaScriptSnippet;

        public ResourceManagementOptionsConfiguration(JavaScriptSnippet javaScriptSnippet) =>
            _javaScriptSnippet = javaScriptSnippet;

        public void Configure(ResourceManagementOptions options)
        {
            var manifest = new ResourceManifest();

            manifest.DefineScript(ResourceNames.TrackingScript).SetInnerContent(_javaScriptSnippet.ScriptBody);

            options.ResourceManifests.Add(manifest);
        }
    }
}
