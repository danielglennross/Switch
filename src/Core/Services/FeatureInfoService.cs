using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Providers;
using Core.Startup;

namespace Core.Services
{
    public class FeatureInfoService : IFeatureInfoService
    {
        private readonly IFeatureProvider _featureProvider;
        private readonly IFeatureManager _featureManager;

        public FeatureInfoService(IFeatureProvider featureProvider, IFeatureManager featureManager)
        {
            _featureManager = featureManager;
            _featureProvider = featureProvider;
        }

        public async Task<IEnumerable<FeatureItem>> GetFeaturesItems()
        {
            var enabledFeatures = 
                await _featureProvider.GetEnabledFeatures().ConfigureAwait(false);

            return _featureManager.FeatureDescriptors.Select(x => new FeatureItem
            {
                FeatureDescriptor = x,
                FeatureState = enabledFeatures.Contains(x.Name) ? FeatureState.Enabled : FeatureState.Disabled
            });
        }
    }
}
