namespace CoreDNX.Startup
{
    public interface IFeatureManifest
    {
        void BuildFeatures(FeatureBuilder builder);
    }
}
