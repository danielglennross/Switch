using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Startup;

namespace Core.Services
{
    public interface IFeatureInfoService
    {
        Task<IEnumerable<FeatureItem>> GetFeaturesItems();
    }
}
