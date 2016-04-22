using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Models;

namespace Core.Services
{
    public interface IFeatureActionService
    {
        Task<IFeature> GetConcreteForInterface(IEnumerable<IFeature> concreteTypes);
        Task<IEnumerable<IFeature>> FilterEnabledFeatures(IEnumerable<IFeature> concreteTypes);
        Task EnableFeature(string name);
        Task DisableFeature(string name);
    }
}
