namespace Core.Startup
{
    public interface IFeatureManifest
    {
        void BuildFeatures(FeatureBuilder builder);
    }
}
