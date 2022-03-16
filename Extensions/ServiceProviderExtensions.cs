using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace System;

internal static class ServiceProviderExtensions
{
    public static HttpContext GetHttpContextSafely(this IServiceProvider serviceProvider)
    {
        try
        {
            return serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext;
        }
        catch (ObjectDisposedException)
        {
            // This happens during a shell restart like when enabling/disabling features.
            return null;
        }
    }
}
