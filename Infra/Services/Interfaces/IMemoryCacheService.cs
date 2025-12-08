
using Infra.Models;
using Infra.Services.Interfaces;

namespace Infra.Interfaces
{
    public interface IMemoryCacheService
    {
        WikiEntity? GetCacheValue(string key, IAppMetricsService appMetricsService);
        void SetCacheValue(string key, WikiEntity wikiEntity, TimeSpan expiration);
    }
}