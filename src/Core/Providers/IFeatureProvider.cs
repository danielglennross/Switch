using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Providers
{
    public interface IFeatureProvider
    {
        Task<IEnumerable<string>> GetEnabledFeatures();
        Task EnableFeature(string feature);
        Task DisableFeature(string feature);
    }
}
