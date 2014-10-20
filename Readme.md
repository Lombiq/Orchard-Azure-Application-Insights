# Hosting - Azure Application Insights Readme



This Orchard module enables easy integration of [Azure Application Insights](http://azure.microsoft.com/en-us/documentation/articles/app-insights-start-monitoring-app-health-usage/) telemetry.

Application Insights NuGet packages are included in the Lib folder. If you want to update them just open the [Hosting - Azure Application Insights Packages](https://bitbucket.org/Lombiq/hosting-azure-application-insights-packages) project, update them there through the NuGet UI and copy over the updated libraries. Note that libraries, unlike how they are created by NuGet, should go into a subfolder whose name doesn't contain their version, to simplify updates. Make sure to apply any changes that are necessary to be made on the client code (like after API changes).