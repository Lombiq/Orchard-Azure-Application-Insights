using Lombiq.Hosting.Azure.ApplicationInsights.Constants;
using Microsoft.ApplicationInsights.AspNetCore;
using OrchardCore.ResourceManagement;

namespace Lombiq.Hosting.Azure.ApplicationInsights
{
    public class ResourceManifest : IResourceManifestProvider
    {
        private readonly JavaScriptSnippet _javaScriptSnippet;

        public ResourceManifest(JavaScriptSnippet javaScriptSnippet) => _javaScriptSnippet = javaScriptSnippet;

        public void BuildManifests(IResourceManifestBuilder builder)
        {
            var manifest = builder.Add();

            manifest.DefineScript(ResourceNames.TrackingScript).SetInnerContent(_javaScriptSnippet.ScriptBody);
        }
    }
}
