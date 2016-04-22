using CoreDNX.Startup;

namespace CoreDNX
{
    public class TestFeatureManifest : IFeatureManifest
    {
        public void BuildFeatures(FeatureBuilder builder)
        {
            builder
                .ForFeature("test1feature", feature => feature.WithDescription("test").WithCategory("test"))
                .ForFeature("test2feature", feature => feature.WithDescription("test").WithCategory("test"))
                .ForFeature("test3feature", feature => feature.WithDescription("test").WithCategory("test"));
        }
    }
}
