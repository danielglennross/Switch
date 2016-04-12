using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Core.Attributes;
using Core.Autofac;
using Core.FeatureRules;
using Core.Models;
using Core.Providers;

namespace Core.Services
{
    public interface IFeatureService
    {
        IFeature GetConcreteForInterface(IEnumerable<IFeature> concreteTypes);
        IEnumerable<IFeature> FilterEnabledFeatures(IEnumerable<IFeature> concreteTypes);
    }

    public class FeatureService : IFeatureService
    {
        private readonly Func<Type, IRule> _ruleFactory;
        private readonly IFeatureProvider _featureProvider;

        public FeatureService(Func<Type, IRule> ruleFactory, IFeatureProvider featureProvider)
        {
            _ruleFactory = ruleFactory;
            _featureProvider = featureProvider;
        }

        public IFeature GetConcreteForInterface(IEnumerable<IFeature> concreteTypes)
        {
            // get concretes and their attributes -> map crete, attr
            // filter by enabled features
            // group by attr desc
            // use first one whos rule evaluates

            var featureWithType = concreteTypes.Select(x => new
            {
                feature = x,
                meta = (FeatureAttrribute)x.GetType().GetCustomAttribute(typeof(FeatureAttrribute)) ?? new NullFeature()
            });

            var enabledFeatures = _featureProvider.GetEnabledFeatures;

            featureWithType = featureWithType.Where(x => enabledFeatures.Contains(x.feature.GetType()));

            featureWithType = featureWithType.OrderByDescending(x => x.meta.Priority);

            var result = featureWithType.First(x => _ruleFactory(x.meta.Rule).Evaluate());

            return result.feature;
        }

        public IEnumerable<IFeature> FilterEnabledFeatures(IEnumerable<IFeature> features)
        {
            var enabledFeatures = _featureProvider.GetEnabledFeatures .Take(2);

            return features.Where(x => enabledFeatures.Contains(x.GetType()));
        }

        class NullFeature : FeatureAttrribute
        {
            public NullFeature() : base(99) { }
        }
    }
}
