
using Infra.Models;

namespace Infra.Interfaces
{
    public interface IMemoryCacheService
    {
        WikiEntity? GetCacheValue(string key);
        void SetCacheValue(string key, WikiEntity wikiEntity, TimeSpan expiration);
    }
}