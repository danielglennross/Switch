using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDNX.Attributes;
using CoreDNX.Autofac;
using CoreDNX.Models;

namespace CoreDNX.Providers
{
    public interface IFeatureProvider
    {
        IEnumerable<Type> GetEnabledSwitches { get; }
        IEnumerable<string> GetEnabledFeatures { get; }
        void EnableFeature(string feature);
        void DisableFeature(string feature);
    }

    public class DefaultFeatureProvider : IFeatureProvider
    {
        public IEnumerable<Type> GetEnabledSwitches => new[] {typeof (Test1), typeof (Test2), typeof (Test3)};

        public IEnumerable<string> GetEnabledFeatures => new[] {"test1feature", "test2feature", "test3feature"};

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
