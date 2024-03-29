# Lombiq Hosting - Azure Application Insights for Orchard Core

[![Lombiq.Hosting.Azure.ApplicationInsights NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Azure.ApplicationInsights?label=Lombiq.Hosting.Azure.ApplicationInsights)](https://www.nuget.org/packages/Lombiq.Hosting.Azure.ApplicationInsights/) [![Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI?label=Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI)](https://www.nuget.org/packages/Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI/)

## About

This [Orchard Core](https://orchardcore.net/) module enables easy integration of [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) telemetry into Orchard. Just install the module, configure the instrumentation key from a configuration source (like the _appsettings.json_ file) as normally for AI, and use the `AddOrchardCoreApplicationInsightsTelemetry()` extension method in your Web project's _Program.cs_ file and collected data will start appearing in the Azure Portal. Check out the module's demo [here](https://www.youtube.com/watch?v=NKKR4R3UPog), and the Orchard Harvest 2023 conference talk about automated QA in Orchard Core [here](https://youtu.be/CHdhwD2NHBU). Note that this module has an Orchard 1 version in the [dev-orchard-1 branch](https://github.com/Lombiq/Orchard-Azure-Application-Insights/tree/dev-orchard-1).

What kind of data is collected from the telemetry and available for inspection in the Azure Portal?

- All usual AI data, including e.g. server-side requests, client-side page views, exceptions and other log entries, dependency calls (like web requests and database queries).
- Information on background task executions (as dependency operations).
- All telemetry is enriched with Orchard-specific and general details like user ID, user name, shell (tenant) name, user agent, IP address.

And all of this can be configured in depth. Extended configuration for built-in AI features is also available, like being able to turn SQL query command text collection on or off.

We at [Lombiq](https://lombiq.com/) also used this module for the following projects:

- The new [City of Santa Monica website](https://santamonica.gov/) when migrating it from Orchard 1 to Orchard Core ([see case study](https://lombiq.com/blog/helping-the-city-of-santa-monica-with-orchard-core-consulting)).
- The new [Lombiq website](https://lombiq.com/) when migrating it from Orchard 1 to Orchard Core ([see case study](https://lombiq.com/blog/how-we-renewed-and-migrated-lombiq-com-from-orchard-1-to-orchard-core)).
- It also makes [DotNest, the Orchard Core SaaS](https://dotnest.com/) better.

Do you want to quickly try out this project and see it in action? Check it out in our [Open-Source Orchard Core Extensions](https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions) full Orchard Core solution and also see our other useful Orchard Core-related open-source projects!

## Documentation

### Setup and basic configuration

Configure the built-in AI options as detailed in the [AI docs](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core#using-applicationinsightsserviceoptions) in an ASP.NET Core configuration source like the _appsettings.json_ file like below (everything in the root `"ApplicationInsights"` section is just built-in AI configuration). Do note that contrary to the standard AI configuration all log entries will be sent to AI by default. If you want to restrict that to just warnings, for example, you also have to add a corresponding `LogLevel` as demonstrated.

```json5
{
  "ApplicationInsights": {
    "ConnectionString": "your connection string comes here"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      //...
    },
    "ApplicationInsights": {
      "LogLevel": {
        "Default": "Warning"
      }
    }
  },
  "OrchardCore": {
    //...
  }
}

```

Add the AI services to the service collection in your Web project's _Program.cs_ file:

```csharp
var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services
    .AddOrchardCms(orchardCoreBuilder =>
    {
        orchardCoreBuilder.AddOrchardCoreApplicationInsightsTelemetry(configuration);
    });
```

Note that due to how the Application Insights .NET SDK works, telemetry can only be collected for the whole app at once; collecting telemetry separately for each tenant is not supported.

When using the full CMS approach of Orchard Core (i.e. not decoupled or headless) then the client-side tracking script will be automatically injected as a head script. Otherwise, you can create it with `ITrackingScriptFactory`.

### Advanced configuration

The module has its own configuration for further options. These need to come from an ASP.NET Core configuration source as well but on the contrary to the basic settings for built-in AI options these need to be put under the `OrchardCore` section, into `Lombiq_Hosting_Azure_ApplicationInsights`:

```json5
{
  "ApplicationInsights": {
    "ConnectionString": "your connection string comes here"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      //...
    }
  },
  "OrchardCore": {
    "Lombiq_Hosting_Azure_ApplicationInsights": {
      "QuickPulseTelemetryModuleAuthenticationApiKey": "your API key here"
    }
  }
}

```

See the [`ApplicationInsightsOptions` class](Lombiq.Hosting.Azure.ApplicationInsights/ApplicationInsightsOptions.cs) for all options and details. We recommend configuring at least `QuickPulseTelemetryModuleAuthenticationApiKey`.

Note that while telemetry from background tasks is collected in form of dependency operations it'll be collected even if `EnableDependencyTrackingTelemetryModule` is `false`.

### Using collected data

All the collected data will be available in the Azure Portal as usual. Some custom properties will be added to all suitable telemetry with the `"OrchardCore."` prefix.

## UI testing

The [`Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI` project](Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI/Readme.md) contains UI test extension methods that you can call from your own test project. Check it out for details.

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
