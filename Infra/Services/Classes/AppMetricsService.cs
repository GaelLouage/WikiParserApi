using Infra.Models;
using Infra.Services.Interfaces;
using Prometheus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Services.Classes
{
    //http://localhost:5000/metrics to call metrics 
    public class AppMetricsService : IAppMetricsService
    {
        private readonly Counter _requests;
        private readonly Counter _cacheHits;
        private readonly Counter _retries;
        private readonly Histogram _responseTime;

        public AppMetricsService()
        {
            _requests = Metrics.CreateCounter(
                "requests_total",
                "Total number of requests");

            _cacheHits = Metrics.CreateCounter(
                "cache_hits_total",
                "Total cache hits");

            _retries = Metrics.CreateCounter(
                "retries_total",
                "Total retry attempts");

            _responseTime = Metrics.CreateHistogram(
                "response_time_seconds",
                "Request processing time");
        }

        public void IncrementRequest() => _requests.Inc();
        public void IncrementCacheHit() => _cacheHits.Inc();
        public void IncrementRetry() => _retries.Inc();

        public IDisposable MeasureResponseTime()
            => _responseTime.NewTimer();

    }
}
