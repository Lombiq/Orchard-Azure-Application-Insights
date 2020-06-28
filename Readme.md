# [Hosting - Azure Application Insights](https://github.com/Lombiq/Orchard-Azure-Application-Insights)



## About

This [Orchard CMS](http://orchardproject.net/) module enables easy integration of [Azure Application Insights](http://azure.microsoft.com/en-us/documentation/articles/app-insights-start-monitoring-app-health-usage/) telemetry into Orchard. Just install the module, configure the instrumentation key from the admin and collected data will start appearing in Application Insights. The module is tenant-aware, so in a multi-tenant setup you can configure different instrumentation keys to collect request tracking and client-side tracking data on different tenants. This is also available on all sites of [DotNest, the Orchard CMS as a Service](https://dotnest.com/).

Warning: this module is only compatible with the Orchard 1.9+.

Note that the module depends on the [Helpful Libraries module](https://github.com/Lombiq/Helpful-Libraries) so you should have that installed as well.

Hosting - Azure Application Insights is part of the [Hosting Suite](https://dotnest.com/knowledge-base/topics/lombiq-hosting-suite), which is a complete Orchard DevOps technology suite for building and maintaining scalable Orchard applications.

The module was created by [Lombiq](http://lombiq.com), one of the core developers of Orchard itself.


## Configuration

You can configure the module, including setting the AI instrumentation key from the admin site, for each tenant. You can also set an application-wide instrumentation key to be used by all tenants (if the module is enabled) in the static configuration (i.e. Web.config, Azure Portal) with the key shown in the `Constants` class.

Even without installing [the AI Status Monitor](https://azure.microsoft.com/en-us/documentation/articles/app-insights-monitor-performance-live-website-now/) (for VMs and local development) or the Application Insights site extension for Azure Web App Services you'll be able to see memory and CPU usage data.


## Extending the module with custom telemetry data

You can send custom events (i.e. totally new events like a user action happening) through a `TelemetryClient` object (you can see examples for this in the [official AI documentation](https://azure.microsoft.com/en-us/documentation/articles/app-insights-api-custom-events-metrics/)). You can create such an object for the current configuration (i.e. what is also used to send request telemetry) through 
`ITelemetryClientFactory.CreateTelemetryClientFromCurrentConfiguration(`).

You can also hook into the default behaviour of the module and e.g. extend what is send during request tracking by implementing the module's event handlers, see the 
`Events` folder/namespace. Particularly you can implement `IRequestTrackingEventHandler` to add custom data to the request telemetry e.g. by adding custom properties to the `Properties` dictionary. Furthermore you can implement `ITelemetryConfigurationEventHandler` to alter the configuration used by any telemetry-sending operation like adding your own `ITelemetryInitializers` to the `TelemetryInitializers` collection.


## Note on assembly binding errors when using dynamic compilation

Note that when you modify this one or a dependent project in the Orchard solution and then refresh a page without doing a manual rebuild (i.e. letting Orchard's dynamic compilation do the job) you can get the following error:

> Could not load file or assembly 'Microsoft.Diagnostics.Tracing.EventSource, Version=1.1.11.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference.

This is because on the fly assembly redirection (see below) doesn't work for some reason in such cases. To solve the issue simply restart the website (from IIS or by restarting IIS Express) after doing a manual build.


## Updating AI libraries

When assembly binding redirects are changed make sure to also edit `AssemblyRedirectSetupShellEventHandler` that mimicks such redirects instead of relying on the Web.config.

Note that since there is a 260 characters limit on paths on Windows, all unused library folders and files should be removed and folders shortened.

After updating you can check for breaking changes between the old and new assembly versions with [BitDiffer](http://www.bitwidgets.com/).


## Contributing and support

Bug reports, feature requests, comments, questions, code contributions, and love letters are warmly welcome, please do so via GitHub issues and pull requests. Please adhere to our [open-source guidelines](https://lombiq.com/open-source-guidelines) while doing so.

This project is developed by [Lombiq Technologies](https://lombiq.com/). Commercial-grade support is available through Lombiq.