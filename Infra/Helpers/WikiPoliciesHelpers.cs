using Infra.Services.Classes;
using Infra.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using Polly.Wrap;
using Prometheus;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Helpers
{
    public static class WikiPoliciesHelpers
    {
        public static AsyncPolicyWrap CreateRetryTimeoutPolicy(IAppMetricsService appMetricsService)
        {
            var retry = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (exception, delay, attempt, ctx) =>
                    {
                        appMetricsService.IncrementRetry(); 
                        Log.Logger.Information($"Retry {attempt} after {delay}. Reason: {exception.Message}");
                    });

            var timeout = Policy.TimeoutAsync(
                TimeSpan.FromSeconds(10),
                TimeoutStrategy.Optimistic,
                onTimeoutAsync: (context, ts, task) =>
                {
                    Log.Logger.Information($"Timeout after {ts.TotalSeconds} seconds.");
                    return Task.CompletedTask;
                });

            return Policy.WrapAsync(retry, timeout);
        }
    }
}
