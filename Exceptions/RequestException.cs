using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Exceptions
{
    /// <summary>
    /// Exception used to wrap exceptions escaping from the request pipeline, then thrown further.
    /// </summary>
    internal class RequestException : Exception
    {
        public RequestException(Exception innerException)
            : base(innerException.Message, innerException)
        {
        }
    }
}