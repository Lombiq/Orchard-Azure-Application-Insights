# Hosting - Azure Application Insights Readme



This Orchard module enables easy integration of [Azure Application Insights](http://azure.microsoft.com/en-us/documentation/articles/app-insights-start-monitoring-app-health-usage/) telemetry.

Note that the module depends on the [Helpful Libraries module](http://helpfullibraries.codeplex.com/) so you should have that installed as well.


## Note on assembly binding errors when using dynamic compilation

Note that when you modify this one or a dependent project in the Orchard solution and then refresh a page without doing a manual rebuild (i.e. letting Orchard's dynamic compilation do the job) you can get the following error:

> Could not load file or assembly 'Microsoft.Diagnostics.Tracing.EventSource, Version=1.1.11.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference.

This is because on the fly assembly redirection (see below) doesn't work for some reason in such cases. To solve the issue simply restart the website (from IIS or by restarting IIS Express) after doing a manual build.


## Updating AI libraries

Application Insights NuGet packages are included in the Lib folder. If you want to update them just open the [Hosting - Azure Application Insights Packages](https://bitbucket.org/Lombiq/hosting-azure-application-insights-packages) project, update them there through the NuGet UI and copy over the updated libraries (you can leave out the nupkg files). Note that libraries, unlike how they are created by NuGet, should go into a subfolder whose name doesn't contain their version, to simplify updates. Make sure to apply any changes that are necessary to be made on the client code (like after API changes).

When assembly binding redirects are changed make sure to also edit `AssemblyRedirectSetupShellEventHandler` that mimicks such redirects instead of relying on the Web.config.

Note that since there is a 260 characters limit on paths on Windows, some unused library folders and files should be removed or renamed:

- Microsoft.ApplicationInsights.RuntimeTelemetry/lib/portable-win81+wpa81
- Microsoft.Diagnostics.Instrumentation.Extensions.Intercept files are in the folder Libs/Intercept
- Microsoft.Diagnostics.Tracing.EventSource.Redist/build/portable-net45+win8+wpa81