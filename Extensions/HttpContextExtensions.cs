using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace System.Web
{
    // Needs to be public to be used in Azure.ApplicationInsights.TrackingScript.cshtml.
    // Yes, this deals with both HttpContextBase and HttpContext.
    public static class HttpContextExtensions
    {
        private const string RequestIdKey = "Lombiq.Hosting.Azure.ApplicationInsights.RequestId";


        public static void SetOperationIdForRequest(this HttpContextBase httpContext, string operationId)
        {
            httpContext.Items[RequestIdKey] = operationId;
        }

        public static string GetOperationIdForRequest(this HttpContextBase httpContext)
        {
            return httpContext.Items[RequestIdKey] as string;
        }

        public static string GetOperationIdForRequest(this HttpContext httpContext)
        {
            return httpContext.Items[RequestIdKey] as string;
        }
    }
}