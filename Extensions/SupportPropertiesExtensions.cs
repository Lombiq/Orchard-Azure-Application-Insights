using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Orchard.Environment.Configuration;

namespace Microsoft.ApplicationInsights.DataContracts
{
    internal static class SupportPropertiesExtensions
    {
        private const string ShellNameKey = "Orchard.ShellName";


        public static void SetShellName(this ISupportProperties supportProperties, ShellSettings shellSettings)
        {
            supportProperties.Properties[ShellNameKey] = shellSettings.Name;
        }

        public static string GetShellName(this ISupportProperties supportProperties)
        {
            string shellName;
            if (!supportProperties.Properties.TryGetValue(ShellNameKey, out shellName)) return null;
            return shellName;
        }
    }
}