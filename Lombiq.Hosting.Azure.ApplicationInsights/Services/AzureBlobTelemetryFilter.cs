using Lombiq.Hosting.Azure.ApplicationInsights.Extensions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using System;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services;

/// <summary>
/// Azure Blob Storage 404s are logged in a parent-child relationship, so we need to ignore the parents also.
/// </summary>
public class AzureBlobTelemetryFilter(ITelemetryProcessor next, IServiceProvider serviceProvider) : ITelemetryProcessor
{
    private string _parentId;

    public void Process(ITelemetry item)
    {
        SetAsIgnoredFailureWhenNeeded(item);

        next.Process(item);
    }

    private void SetAsIgnoredFailureWhenNeeded(ITelemetry item)
    {
        if (item is not DependencyTelemetry { Success: false } dependency) return;

        if (dependency.ResultCode == "404" && dependency.Type == "Azure blob" &&
            dependency.ShouldSetAsIgnoredFailure(serviceProvider))
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
