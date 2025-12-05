using Infra.Interfaces;
using Infra.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime;

namespace Infra.Classes
{
    public class MemoryCacheService : IMemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public void SetCacheValue(string key, WikiEntity wikiEntity, TimeSpan expiration)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            };

            _memoryCache.Set(key, wikiEntity, options);
        }

        public WikiEntity? GetCacheValue(string key)
        {
            _memoryCache.TryGetValue(key, out WikiEntity? wikiEntity);
            return wikiEntity;
        }
    }
}
