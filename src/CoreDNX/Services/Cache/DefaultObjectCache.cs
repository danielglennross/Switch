using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreDNX.Services.Cache
{
    public class DefaultObjectCache : IInterceptCache
    {
        private readonly IDictionary<string, object> _cache;

        public DefaultObjectCache()
        {
            _cache = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
        }

        public string GetEnumerableCacheKey(string interfaceName) => interfaceName + "_Enumerable_Default";
        public string GetSwitchCacheKey(string interfaceName) => interfaceName + "_Switch_Default";

        public async Task<T> Get<T>(string key, Func<Task<T>> factory)
        {
            if (_cache.ContainsKey(key))
            {
                return (T) (_cache[key] ?? default(T));
            }

            var computed = await factory().ConfigureAwait(false);
            _cache.Add(key, computed);
            return computed;
        }

        public Task Remove(string key)
        {
            if (_cache.ContainsKey(key))
            {
                _cache.Remove(key);
            }
            return Task.FromResult(0);
        }
    }
}
