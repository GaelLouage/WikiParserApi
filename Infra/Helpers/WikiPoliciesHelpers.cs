using Microsoft.Extensions.Logging;
using Polly;
using Polly.Timeout;
using Polly.Wrap;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Helpers
{
    public static class WikiPoliciesHelpers
    {
        public static AsyncPolicyWrap CreateRetryTimeoutPolicy()
        {
            var messageSb = new StringBuilder();
            var retry = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt =>
                        TimeSpan.FromSeconds(Math.Pow(2, attempt)), // 2s, 4s, 8s
                    onRetry: (exception, delay, attempt, ctx) =>
                    {
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
