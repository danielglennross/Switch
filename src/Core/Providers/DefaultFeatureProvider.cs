using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Autofac;

namespace Core.Providers
{
    public interface IFeatureProvider
    {
        IEnumerable<Type> GetEnabledFeatures { get; }
        void EnableFeature(string feature);
        void DisableFeature(string feature);
    }

    public class DefaultFeatureProvider : IFeatureProvider
    {
        public IEnumerable<Type> GetEnabledFeatures => new[] {typeof (Test1), typeof (Test2), typeof (Test3)};

        public void DisableFeature(string feature)
        {
            throw new NotImplementedException();
        }

        public void EnableFeature(string feature)
        {
            throw new NotImplementedException();
        }
    }
}
