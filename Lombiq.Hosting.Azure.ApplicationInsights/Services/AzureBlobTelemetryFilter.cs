using Lombiq.Hosting.Azure.ApplicationInsights.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

public class AzureBlobTelemetryFilter : ITelemetryProcessor
{
    private readonly ITelemetryProcessor _next;
    private readonly IServiceProvider _serviceProvider;
    private string _parentId;

    public AzureBlobTelemetryFilter(ITelemetryProcessor next, IServiceProvider serviceProvider)
    {
        _next = next;
        _serviceProvider = serviceProvider;
    }

    public void Process(ITelemetry item)
    {
        SetAsIgnoredFailureWhenNeeded(item);

        _next.Process(item);
    }

    private void SetAsIgnoredFailureWhenNeeded(ITelemetry item)
    {
        if (item is not DependencyTelemetry { Success: false } dependency) return;

        if (dependency.ResultCode == "404" && dependency.Type == "Azure blob" &&
            dependency.ShouldSetAsIgnoredFailure(_serviceProvider))
        {
            _parentId = dependency.Context.Operation.ParentId;
            dependency.SetAsIgnoredFailure();
            return;
        }

        if (_parentId == dependency.Id && dependency.Name is "Blob.GetProperties" or "BlobBaseClient.GetProperties")
        {
            _parentId = dependency.Context.Operation.ParentId;
            dependency.SetAsIgnoredFailure();
        }
    }
}
