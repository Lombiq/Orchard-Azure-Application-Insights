# Lombiq Hosting - Azure Application Insights for Orchard Core

[![Lombiq.Hosting.Azure.ApplicationInsights NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Azure.ApplicationInsights?label=Lombiq.Hosting.Azure.ApplicationInsights)](https://www.nuget.org/packages/Lombiq.Hosting.Azure.ApplicationInsights/) [![Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI NuGet](https://img.shields.io/nuget/v/Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI?label=Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI)](https://www.nuget.org/packages/Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI/)

## About

This [Orchard Core](https://orchardcore.net/) module enables easy integration of [Azure Application Insights](https://docs.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview) telemetry into Orchard. Just install the module, configure the instrumentation key from a configuration source (like the _appsettings.json_ file) as normally for AAI, and use the `AddOrchardCoreApplicationInsightsTelemetry()` extension method in your Web project's _Program.cs_ file and collected data will start appearing in the Azure Portal. Check out the module's demo [here](https://www.youtube.com/watch?v=NKKR4R3UPog), and the Orchard Harvest 2023 conference talk about automated QA in Orchard Core [here](https://youtu.be/CHdhwD2NHBU). Note that this module has an Orchard 1 version in the [dev-orchard-1 branch](https://github.com/Lombiq/Orchard-Azure-Application-Insights/tree/dev-orchard-1).

What kind of data is collected from the telemetry and available for inspection in the Azure Portal?

- All usual AAI data, including e.g. server-side requests, client-side page views, exceptions and other log entries, dependency calls (like web requests and database queries).
- Information on background task executions (as dependency operations).
- All telemetry is enriched with Orchard-specific and general details like user ID, user name, shell (tenant) name, user agent, IP address.

And all of this can be configured in depth. Extended configuration for built-in AAI features is also available, like being able to turn SQL query command text collection on or off.

We at [Lombiq](https://lombiq.com/) also used this module for the following projects:

- The new [City of Santa Monica website](https://santamonica.gov/) when migrating it from Orchard 1 to Orchard Core ([see case study](https://lombiq.com/blog/helping-the-city-of-santa-monica-with-orchard-core-consulting)).
- The new [Lombiq website](https://lombiq.com/) when migrating it from Orchard 1 to Orchard Core ([see case study](https://lombiq.com/blog/how-we-renewed-and-migrated-lombiq-com-from-orchard-1-to-orchard-core)).
- It also makes [DotNest, the Orchard Core SaaS](https://dotnest.com/) better.

Do you want to quickly try out this project and see it in action? Check it out in our [Open-Source Orchard Core Extensions](https://github.com/Lombiq/Open-Source-Orchard-Core-Extensions) full Orchard Core solution and also see our other useful Orchard Core-related open-source projects!

## Documentation

### Setup and basic configuration

Configure the built-in AAI options as detailed in the [AAI docs](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core#using-applicationinsightsserviceoptions) in an ASP.NET Core configuration source like the _appsettings.json_ file like below (everything in the root `"ApplicationInsights"` section is just built-in AAI configuration). Do note that contrary to the standard AAI configuration all log entries will be sent to AAI by default. If you want to restrict that to just warnings, for example, you also have to add a corresponding `LogLevel` as demonstrated.

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

Add the AAI services to the service collection in your Web project's _Program.cs_ file:

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

You can also use `ConfigureAzureHostingDefaultsWithApplicationInsightsTelemetry` instead; this sets up all the recommended hosting configuration with `ConfigureAzureHostingDefaults` from [Lombiq Helpful Libraries - Orchard Core Libraries](https://github.com/Lombiq/Helpful-Libraries/blob/dev/Lombiq.HelpfulLibraries.OrchardCore/Readme.md).

When using the full CMS approach of Orchard Core (i.e. not decoupled or headless) then the client-side tracking script will be automatically injected as a head script. Otherwise, you can create it with `ITrackingScriptFactory`.

### Advanced configuration

The module has its own configuration for further options. These need to come from an ASP.NET Core configuration source as well but on the contrary to the basic settings for built-in AAI options these need to be put under the `OrchardCore` section, into `Lombiq_Hosting_Azure_ApplicationInsights`:

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
      "EnableUserNameCollection": true,
      "EnableLoggingTestMiddleware": true
    }
  }
}

```

See the [`ApplicationInsightsOptions` class](Lombiq.Hosting.Azure.ApplicationInsights/ApplicationInsightsOptions.cs) for all options and details.

Note that while telemetry from background tasks is collected in form of dependency operations it'll be collected even if `EnableDependencyTrackingTelemetryModule` is `false`.

If you use the security defaults from [Lombiq Helpful Libraries - Orchard Core Libraries - Security](https://github.com/Lombiq/Helpful-Libraries/blob/dev/Lombiq.HelpfulLibraries.OrchardCore/Docs/Security.md), then the security headers necessary to use Application Insight's client-side tracking will automatically be added.

### Entra Authentication for the Live Metrics control channel

To [secure the Live Metrics control channel](https://learn.microsoft.com/en-us/azure/azure-monitor/app/live-stream#secure-the-control-channel), you'll have to set up Entra Authentication for it. You may omit this if not needed; configuring the connection string is necessary in any case, and enough for simply collecting telemetry. Entra Authentication is only needed if you want to control the Live Metrics stream from the Azure Portal, like filtering telemetry.

#### Setting up Entra Authentication for Application Insights

> ⚠ This section is required if you have disabled `Local Authentication` on your AAI resource, see [the docs](https://learn.microsoft.com/en-us/azure/azure-monitor/app/azure-ad-authentication?WT.mc_id=Portal-AppInsightsExtension&tabs=net#disable-local-authentication).

If you want to use Entra Authentication for Application Insights, or if you have disabled `Local Authentication` on your AAI resource, you will have to set the `EntraAuthenticationType` option to the authentication type you want to use (`ManagedIdentity` or `ServicePrincipal`) in the `Lombiq_Hosting_Azure_ApplicationInsights` section of your configuration like below.

```json5
{
    "OrchardCore": {
        "Lombiq_Hosting_Azure_ApplicationInsights": {
            "EntraAuthenticationType": "ManagedIdentity"
        }
    }
}
```

If you use `ConfigureAzureHostingDefaultsWithApplicationInsightsTelemetry` as mentioned above, then this will be automatically configured for you.

> ⚠ Client-side tracking will currently fail with 401 Unauthorized if Local Authentication is disabled, see [this bug report](https://github.com/microsoft/ApplicationInsights-dotnet/issues/2893) for the Application Insights .NET SDK. If you need client-side tracking, you will have to keep Local Authentication enabled on your AAI resource for now.

To set up Entra Authentication for an application hosted on Azure you will have to set up a Managed Identity for the application and give it the `Monitoring Metrics Publisher` role under the given AAI resource (see more on assigning Azure roles [here](https://learn.microsoft.com/en-us/azure/role-based-access-control/role-assignments-portal)) to be able to publish metrics. A managed identity will allow your app to authenticate with the Application Insights resource; see how to set it up for specific Azure services [here](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/managed-identities-status). We recommend using the simpler system-assigned identity option, since then you can easily allow your app's identity to get a role under the Application Insights resource. Note that it might take a few minutes for the managed identity to work; until then, Live Metrics won't be available.

You can also use a service principal to authenticate. To set this up, you will have to provide the service principal credentials in the configuration. See the [Service principal](#service-principal) section for more information. This is also the only way to authenticate if you are using a non-Azure (or local) environment - or an Azure resource that does not support Managed Identities.

Once Entra Authentication is set up and the `ConnectionString` has been properly set, you should be able to control the Live Metrics stream from the Azure Portal, like filtering telemetry.

#### Service principal

Using a Service Principal is the only way to authenticate using Entra authentication if you are using a non-Azure (or local) environment.

If you want to use the Service Principal method for your Application Insights resource, you should set the `EntraAuthenticationType` option to `ServicePrincipal` in the `Lombiq_Hosting_Azure_ApplicationInsights` section of your configuration. To securely control the Live Metrics stream with Entra ID you will also have to provide the credentials of the service principal; to set this up, [see the docs](https://learn.microsoft.com/en-us/entra/identity-platform/howto-create-service-principal-portal) (you'll need to use the [client secret authentication option](https://learn.microsoft.com/en-us/entra/identity-platform/howto-create-service-principal-portal#option-3-create-a-new-client-secret)).

```json5
{
  "OrchardCore": {
    "Lombiq_Hosting_Azure_ApplicationInsights": {
        "EntraAuthenticationType": "ServicePrincipal",
        "ServicePrincipalCredentials": {
            "TenantId": "your service principal tenant id",
            "ClientId": "your service principal client id",
            "ClientSecret": "your service principal client secret"
        }
    }
  }
}
```

For more information or scenarios not described here, see the [official documentation](https://learn.microsoft.com/en-us/azure/azure-monitor/app/azure-ad-authentication).

### Using collected data

All the collected data will be available in the Azure Portal as usual. Some custom properties will be added to all suitable telemetry with the `"OrchardCore."` prefix.

### Privacy considerations and GDPR

Depending on what data you configure Application Insights to collect, and your jurisdiction, you might have to inform your users about the collected data and/or ask for their consent. For example, if you collect user IDs or IP addresses, then you will likely have to inform your users, even if not necessarily ask for consent assuming the purpose is strictly application performance monitoring. Please consult with a legal expert on this matter.

The client-side tracking script will not use cookies if `ITrackingConsentFeature` is used and no consent was given.

To help with asking users for consent, check out our [Lombiq Privacy for Orchard Core](https://github.com/Lombiq/Orchard-Privacy) module.

### Profiling

Support for [Application Insights Profiler for ASP.NET Core](https://github.com/microsoft/ApplicationInsights-Profiler-AspNetCore) is built in. This means that you can just launch the [Application Insights Profiler](https://learn.microsoft.com/en-us/azure/azure-monitor/optimization-insights/code-optimizations-profiler-overview) from the Azure Portal if you'd like to profile the app. To enable this, you first need to set the `EnableProfiler` option to `true` in the `Lombiq_Hosting_Azure_ApplicationInsights` section of your configuration like below.

```json5
{
  "OrchardCore": {
    "Lombiq_Hosting_Azure_ApplicationInsights": {
        "EnableProfiler": true
    }
  }
}
```

## UI testing

The [`Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI` project](Lombiq.Hosting.Azure.ApplicationInsights.Tests.UI/Readme.md) contains UI test extension methods that you can call from your own test project. Check it out for details.

## Contributing and support

Bug reports, feature requests, comments, questions, code contributions and love letters are warmly welcome. You can send them to us via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.
