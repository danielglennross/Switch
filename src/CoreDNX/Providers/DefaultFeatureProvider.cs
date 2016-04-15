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
        private readonly IList<string> _enabledFeatures;

        public DefaultFeatureProvider()
        {
            _enabledFeatures = new List<string>();
        }

        public IEnumerable<Type> GetEnabledSwitches => new[] {typeof (Test1), typeof (Test2), typeof (Test3)};

        public IEnumerable<string> GetEnabledFeatures => _enabledFeatures;

        public void DisableFeature(string feature)
        {
            _enabledFeatures.Remove(feature);
        }

        public void EnableFeature(string feature)
        {
            _enabledFeatures.Add(feature);
        }
    }
}
