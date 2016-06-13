using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDNX.Models;
using CoreDNX.Services.Cache;
using CoreDNX.Startup;

namespace CoreDNX.Services.Events
{
    public class CacheInvalidatorEvent : ICacheEvents
    {
        private readonly IInterceptCache _cache;
        private readonly IFeatureInfoService _featureInfoService;

        public CacheInvalidatorEvent(
            IInterceptCache cache,
            IFeatureInfoService featureInfoService)
        {
            _featureInfoService = featureInfoService;
            _cache = cache;
        }

        public async Task OnFeatureEnabled(string name)
        {
            await InvalidateCache(name, true).ConfigureAwait(false);
        }

        public async Task OnFeatureDisabled(string name)
        {
            await InvalidateCache(name, false).ConfigureAwait(false);
        }

        public async Task InvalidateCache(string name, bool findDeepInterfacesToInvalidate)
        {
            var featureItems = (await _featureInfoService.GetFeaturesItems().ConfigureAwait(false)).ToList();
            var feature = featureItems.SingleOrDefault(x => x.FeatureDescriptor.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            var interfaces = new List<string>();
            GetAllInterfaces(interfaces, featureItems, feature, findDeepInterfacesToInvalidate);

            var switchCacheTasks = interfaces.Select(x => _cache.Remove(_cache.GetSwitchCacheKey(x)));
            var enumerableCacheTasks = interfaces.Select(x => _cache.Remove(_cache.GetEnumerableCacheKey(x)));

            await Task.WhenAll(switchCacheTasks.Concat(enumerableCacheTasks)).ConfigureAwait(false);
        }

        private static void GetAllInterfaces(List<string> interfaces, IList<FeatureItem> allFeatures, FeatureItem currentFeature, bool deep)
        {
            var interfacesFromFeature = currentFeature.FeatureDescriptor.ExportedTypes
                .SelectMany(x => x.GetInterfaces())
                .Where(x => typeof(IFeature).IsAssignableFrom(x) && x != typeof(IFeature))
                .Select(x => x.Name);

            interfaces.AddRange(interfacesFromFeature);

            var dependencies = currentFeature.FeatureDescriptor.Dependencies.ToList();
            if (deep && dependencies.Any())
            {
                dependencies.ForEach(x => 
                    GetAllInterfaces(interfaces, allFeatures, 
                        allFeatures.SingleOrDefault(y => y.FeatureDescriptor.Name.Equals(x, StringComparison.OrdinalIgnoreCase)), true));
            }
        }
    }
}
