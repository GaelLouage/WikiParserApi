using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Services.Interfaces
{
    public interface IAppMetricsService
    {
        void IncrementRequest();
        void IncrementCacheHit();
        void IncrementRetry();
        IDisposable MeasureResponseTime();
    }
}
