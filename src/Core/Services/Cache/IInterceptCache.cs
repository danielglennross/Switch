using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Services.Cache
{
    public interface IInterceptCache
    {
        string GetEnumerableCacheKey(string interfaceName);
        string GetSwitchCacheKey(string interfaceName);
        Task<T> Get<T>(string key, Func<Task<T>> factory);
        Task Remove(string key);
    }

    interface ICacheEvents
    {
        Task OnFeatureEnabled(string featureName);
        Task OnFeatureDisabled(string featureName);
    }
}
