# Lombiq Hosting - Azure Application Insights



## About

This [Orchard Core](https://www.orchardcore.net/) module enables easy integration of [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) telemetry into Orchard. Just install the module, configure the instrumentation key from a configuration source (like the *appsettings.json* file) as normally for AI, and collected data will start appearing in the Azure Portal.

Note that the module depends on [Helpful Libraries](https://github.com/Lombiq/Helpful-Libraries/).


## Documentation

### Setup and basic configuration

Configure the built-in AI options as detailed in the [AI docs](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core#using-applicationinsightsserviceoptions) in an ASP.NET Core configuration source like the *appsettings.json* file like below. Do note that contrary to the standard AI configuration all log entries will be send to AI by default. If you want to restrict that to just warnings, for example, you also have to add a corresponding `LogLevel` as demonstrated.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      ...
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  },
  "OrchardCore": {
    ...
  },
  "ApplicationInsights": {
    "InstrumentationKey": "your instrumentation key comes here"
  }
}

```

In a multi-tenant setup you can configure different instrumentation keys to collect request tracking and client-side tracking data on different tenants, just follow [the Orchard Core configuration docs](https://docs.orchardcore.net/en/dev/docs/reference/core/Configuration/).

When using the full CMS approach of Orchard Core (i.e. not decoupled or headless) then the client-side tracking script will be automatically injected as a head script. Otherwise, you can reference and require it as `"Lombiq.Hosting.Azure.ApplicationInsights.TrackingScript"`. 

### Advanced configuration

The module has its own configuration for further options. These need to come from an ASP.NET Core configuration source as well but on the contrary to the basic settings for built-in AI options these need to be put under the `OrchardCore` section, into `Lombiq_Hosting_Azure_ApplicationInsights`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      ...
    }
  },
  "OrchardCore": {
    "Lombiq_Hosting_Azure_ApplicationInsights": {
      "EnableSqlCommandTextInstrumentation":  false
    }
  },
  "ApplicationInsights": {
    "InstrumentationKey": "your instrumentation key comes here"
  }
}

```

See the [`ApplicationInsightsOptions` class](Configuration/ApplicationInsightsOptions.cs) for all options.


## Contributing and support

Bug reports, feature requests, comments, questions, code contributions, and love letters are warmly welcome, please do so via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
