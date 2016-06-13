using System;
using System.Collections.Generic;

namespace CoreDNX.Startup
{
    public interface IFeatureManager
    {
        IEnumerable<FeatureDescriptor> FeatureDescriptors { get; }
    }

    public class FeatureManager : IFeatureManager
    {
        private readonly Lazy<IEnumerable<FeatureDescriptor>> _featureDescriptors;
        private readonly IEnumerable<IFeatureManifest> _featureManifests;

        public IEnumerable<FeatureDescriptor> FeatureDescriptors => _featureDescriptors.Value; 

        public FeatureManager(IEnumerable<IFeatureManifest> featureManifests)
        {
            _featureManifests = featureManifests;
            _featureDescriptors = new Lazy<IEnumerable<FeatureDescriptor>>(GetFeaturesDescriptors);
        }

        public IEnumerable<FeatureDescriptor> GetFeaturesDescriptors()
        {
            var builder = new FeatureBuilder();
            foreach (var featureManifest in _featureManifests)
            {
                featureManifest.BuildFeatures(builder);
            }
            return builder.Build();
        } 
    }
}
