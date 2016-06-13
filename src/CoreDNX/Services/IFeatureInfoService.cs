using System.Collections.Generic;
using System.Threading.Tasks;
using CoreDNX.Startup;

namespace CoreDNX.Services
{
    public interface IFeatureInfoService
    {
        Task<IEnumerable<FeatureItem>> GetFeaturesItems();
    }
}
