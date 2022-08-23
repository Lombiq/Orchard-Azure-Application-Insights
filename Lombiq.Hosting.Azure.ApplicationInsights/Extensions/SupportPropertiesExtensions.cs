using System.Collections.Generic;

namespace Microsoft.ApplicationInsights.DataContracts;

public static class SupportPropertiesExtensions
{
    public static bool TryAddProperty(this ISupportProperties supportProperties, string key, string value) =>
        supportProperties.Properties.TryAdd("OrchardCore." + key, value);
}
