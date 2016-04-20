using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CoreDNX.Attributes;
using CoreDNX.Autofac;
using CoreDNX.FeatureRules;
using CoreDNX.Models;
using CoreDNX.Providers;
using CoreDNX.Startup;

namespace CoreDNX.Services
{
    public interface IFeatureActionService
    {
        ISwitch GetConcreteForInterface(IEnumerable<ISwitch> concreteTypes);
        IEnumerable<ISwitch> FilterEnabledFeatures(IEnumerable<ISwitch> concreteTypes);

        IFeature GetConcreteForInterface(IEnumerable<IFeature> concreteTypes);
        IEnumerable<IFeature> FilterEnabledFeatures(IEnumerable<IFeature> concreteTypes);

        void EnableFeature(string name);
        void DisableFeature(string name);
    }

    public interface IFeatureInfoService
    {
        IEnumerable<FeatureItem> GetFeaturesItems();
    }

    public class FeatureInfoService : IFeatureInfoService
    {
        private readonly IFeatureProvider _featureProvider;
        private readonly IFeatureManager _featureManager;

        public FeatureInfoService(IFeatureProvider featureProvider, IFeatureManager featureManager)
        {
            _featureManager = featureManager;
            _featureProvider = featureProvider;
        }

        public IEnumerable<FeatureItem> GetFeaturesItems()
        {
            return _featureManager.FeatureDescriptors.Select(x => new FeatureItem
            {
                FeatureDescriptor = x,
                FeatureState = _featureProvider.GetEnabledFeatures.Contains(x.Name) ? FeatureState.Enabled : FeatureState.Disabled
            });
        }
    }

    public class FeatureActionService : IFeatureActionService
    {
        private readonly Func<Type, IRule> _ruleFactory;
        private readonly IFeatureProvider _featureProvider;
        private readonly IFeatureManager _featureManager;

        public FeatureActionService(Func<Type, IRule> ruleFactory, IFeatureProvider featureProvider, IFeatureManager featureManager)
        {
            _featureManager = featureManager;
            _ruleFactory = ruleFactory;
            _featureProvider = featureProvider;
        }

        public ISwitch GetConcreteForInterface(IEnumerable<ISwitch> concreteTypes)
        {
            // get concretes and their attributes -> map crete, attr
            // filter by enabled features
            // group by attr desc
            // use first one whos rule evaluates

            var featureWithType = concreteTypes.Select(x => new
            {
                feature = x,
                meta = (SwitchAttrribute)x.GetType().GetCustomAttribute(typeof(SwitchAttrribute)) ?? new NullSwitch()
            }).ToList();

            var enabledSwitches = _featureProvider.GetEnabledSwitches;

            featureWithType = featureWithType.Where(x => enabledSwitches.Contains(x.feature.GetType())).ToList();

            if (!featureWithType.Any())
            {
                throw new InvalidOperationException("No features are enabled that contain this type");
            }

            featureWithType = featureWithType.OrderByDescending(x => x.meta.Priority).ToList();

            var result = featureWithType.First(x => _ruleFactory(x.meta.Rule).Evaluate());

            return result.feature;
        }

        public IEnumerable<ISwitch> FilterEnabledFeatures(IEnumerable<ISwitch> features)
        {
            var enabledSwitches = _featureProvider.GetEnabledSwitches;

            return features.Where(x => enabledSwitches.Contains(x.GetType()));
        }

        class NullSwitch : SwitchAttrribute
        {
            public NullSwitch() : base(99) { }
        }

        class NullFeature : FeatureAttribute
        {
            public NullFeature() : base("") { }
        }

        class NullSuppressedType : SuppressTypeAttribute
        {
            public NullSuppressedType() : base(null) { }
        }



        public IFeature GetConcreteForInterface(IEnumerable<IFeature> concreteTypes)
        {
            var type = FilterEnabledFeatures(concreteTypes).FirstOrDefault();
            if (type == null)
            {
                throw new InvalidOperationException("No features are enabled that contain this type");
            }
            return type;
        }

        public IEnumerable<IFeature> FilterEnabledFeatures(IEnumerable<IFeature> concreteTypes)
        {
            var featureWithType = concreteTypes.Select(x => new
            {
                feature = x,
                featMeta = (FeatureAttribute)x.GetType().GetCustomAttribute(typeof(FeatureAttribute)) ?? new NullFeature(),
                suppMeta = (SuppressTypeAttribute)x.GetType().GetCustomAttribute(typeof(SuppressTypeAttribute)) ?? new NullSuppressedType(),
            }).ToList();

            var activeFeatures = featureWithType.Where(x => _featureProvider.GetEnabledFeatures.Contains(x.featMeta.Name)).ToList();

            var suppressedTypes = activeFeatures.Select(x => x.suppMeta.Type).Where(x => x != null).ToList();

            var starts = activeFeatures.Where(x => !suppressedTypes.Contains(x.feature.GetType())).ToList();

            var tt = new List<IFeature>();
            starts.ForEach(start =>
            {
                while (true)
                {
                    if (start == null || _ruleFactory(start.suppMeta.RuleType).Evaluate()) break;
                    start = activeFeatures.First(x => x.feature.GetType() == start.suppMeta.Type);
                }
                if (start != null) tt.Add(start.feature);
            });

            return tt;
        }

        public void EnableFeature(string name)
        {
            var featureDesc =
                _featureManager.FeatureDescriptors.FirstOrDefault(
                    x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            var featuresToEnable =
                featureDesc.Dependencies.Concat(new[] {featureDesc.Name})
                    .Where(x => !_featureProvider.GetEnabledFeatures.Contains(x));

            foreach (var feature in featuresToEnable)
            {
                _featureProvider.EnableFeature(feature);
            }
        }

        public void DisableFeature(string name)
        {
            _featureProvider.DisableFeature(name);
        }
    }
}
