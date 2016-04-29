using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Core.Attributes;
using Core.FeatureRules;
using Core.Models;
using Core.Providers;
using Core.Startup;

namespace Core.Services
{
    public delegate Task OnFeatureEnabled(string featureName);
    public delegate Task OnFeatureDisabled(string featureName);

    public class FeatureActionService : IFeatureActionService
    {
        public event OnFeatureEnabled OnFeatureEnabled = delegate { return Task.CompletedTask; };
        public event OnFeatureDisabled OnFeatureDisabled = delegate { return Task.CompletedTask; };

        private readonly Func<Type, IRule> _ruleFactory;
        private readonly IFeatureProvider _featureProvider;
        private readonly IFeatureManager _featureManager;

        public FeatureActionService(Func<Type, IRule> ruleFactory, IFeatureProvider featureProvider, IFeatureManager featureManager)
        {
            _featureManager = featureManager;
            _ruleFactory = ruleFactory;
            _featureProvider = featureProvider;
        }

        class NullFeature : FeatureAttribute
        {
            public NullFeature() : base("") { }
        }

        class NullSuppressedType : SuppressTypeAttribute
        {
            public NullSuppressedType() : base(null) { }
        }

        public async Task<IFeature> GetConcreteForInterface(IEnumerable<IFeature> concreteTypes)
        {
            var enabledFeatures = await FilterEnabledFeatures(concreteTypes).ConfigureAwait(false);
            var type = enabledFeatures.FirstOrDefault();
            if (type == null)
            {
                throw new InvalidOperationException("No features are enabled that contain this type");
            }
            return type;
        }

        public async Task<IEnumerable<IFeature>> FilterEnabledFeatures(IEnumerable<IFeature> concreteTypes)
        {
            var featureWithType = concreteTypes.Select(x => new
            {
                feature = x,
                featMeta = (FeatureAttribute)x.GetType().GetCustomAttribute(typeof(FeatureAttribute)) ?? new NullFeature(),
                suppMeta = (SuppressTypeAttribute)x.GetType().GetCustomAttribute(typeof(SuppressTypeAttribute)) ?? new NullSuppressedType(),
            }).ToList();

            var enabledFeatures =
                await _featureProvider.GetEnabledFeatures().ConfigureAwait(false);

            var activeFeatures = featureWithType
                .Where(x => enabledFeatures.Contains(x.featMeta.Name))
                .ToList();

            var suppressedTypes = activeFeatures
                .Select(x => x.suppMeta.Type)
                .Where(x => x != null).
                ToList();

            var starts = activeFeatures
                .Where(x => !suppressedTypes.Contains(x.feature.GetType()))
                .ToList();

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

        public async Task EnableFeature(string name)
        {
            var featureDesc =
                _featureManager.FeatureDescriptors.FirstOrDefault(
                    x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            var enabledFeatures = 
                await _featureProvider.GetEnabledFeatures().ConfigureAwait(false);

            var featuresToEnable =
                featureDesc.Dependencies.Concat(new[] { featureDesc.Name })
                    .Where(x => !enabledFeatures.Contains(x))
                    .ToList();

            var featuresToEnableTasks = featuresToEnable.Select(_featureProvider.EnableFeature);
            await Task.WhenAll(featuresToEnableTasks).ConfigureAwait(false);

            await OnFeatureEnabled(name).ConfigureAwait(false);
        }

        public async Task DisableFeature(string name)
        {
            await _featureProvider.DisableFeature(name).ConfigureAwait(false);

            await OnFeatureDisabled(name).ConfigureAwait(false);
        }
    }
}
