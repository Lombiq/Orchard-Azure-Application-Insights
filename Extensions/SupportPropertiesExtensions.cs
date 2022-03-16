using System.Collections.Generic;

namespace Microsoft.ApplicationInsights.DataContracts;

public static class SupportPropertiesExtensions
{
    public static void TryAddProperty(this ISupportProperties supportProperties, string key, string value) =>
        supportProperties.Properties.TryAdd("OrchardCore." + key, value);
}
