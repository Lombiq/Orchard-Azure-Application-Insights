using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Orchard
{
    internal static class WorkContextExtensions
    {
        private const string AuthenticatedUserIdHttpItemsKey = "Lombiq.Hosting.Azure.ApplicationInsight.AuthenticatedUserId";


        public static void SetAuthenticatedUserIdForRequest(this WorkContext workContext)
        {
            workContext.HttpContext.Items[AuthenticatedUserIdHttpItemsKey] = workContext.CurrentUser?.Id.ToString();
        }

        public static string GetAuthenticatedUserIdForRequest(this WorkContext workContext)
        {
            return workContext.HttpContext.Items[AuthenticatedUserIdHttpItemsKey] as string;
        }
    }
}