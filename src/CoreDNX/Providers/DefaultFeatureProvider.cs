using System.Collections.Generic;
using System.Threading.Tasks;

namespace CoreDNX.Providers
{
    public class DefaultFeatureProvider : IFeatureProvider
    {
        private readonly IList<string> _enabledFeatures;

        public DefaultFeatureProvider()
        {
            _enabledFeatures = new List<string>();
        }

        public Task<IEnumerable<string>> GetEnabledFeatures() => Task.FromResult(_enabledFeatures as IEnumerable<string>);

        public Task DisableFeature(string feature)
        {
            _enabledFeatures.Remove(feature);
            return Task.FromResult(0);
        }

        public Task EnableFeature(string feature)
        {
            _enabledFeatures.Add(feature);
            return Task.FromResult(0);
        }
    }
}
