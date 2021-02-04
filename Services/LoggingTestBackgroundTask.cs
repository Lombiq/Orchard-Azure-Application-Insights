using Lombiq.Hosting.Azure.ApplicationInsights.Helpers;
using Microsoft.ApplicationInsights;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.BackgroundTasks;
using OrchardCore.Modules;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Lombiq.Hosting.Azure.ApplicationInsights.Services
{
    [BackgroundTask(Schedule = "* * * * *", Description = "Sends scheduled emails.")]
    public class LoggingTestBackgroundTask : IBackgroundTask
    {
        public Task DoWorkAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            LoggingTestHelper.LogTestMessagesAndThrow(
                serviceProvider.GetRequiredService<ILogger<LoggingTestBackgroundTask>>(),
                serviceProvider.GetRequiredService<IClock>(),
                serviceProvider.GetRequiredService<TelemetryClient>());

            return Task.CompletedTask;
        }
    }
}
