using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreDNX.Providers;

namespace CoreDNX.Startup
{
    public interface IFeatureManifest
    {
        void BuildFeatures(FeatureBuilder builder);
    }

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

    public class FeatureBuilder
    {
        private readonly IList<FeatureDescriptor> _features;

        public FeatureBuilder()
        {
            _features = new List<FeatureDescriptor>();
        }

        public FeatureBuilder ForFeature(string name, Action<FeatureItem> featureFactory)
        {
            var builder = new FeatureItem(name);
            featureFactory(builder);

            _features.Add(builder.Build());
            return this;
        }
        public IEnumerable<FeatureDescriptor> Build()
        {
            //build proper connections
            return _features ?? Enumerable.Empty<FeatureDescriptor>();
        }

        public class FeatureItem
        {
            private FeatureDescriptor _featureDescriptor;

            public FeatureItem(string name)
            {
                _featureDescriptor = new FeatureDescriptor
                {
                    Name = name,
                    Dependencies = new List<string>()
                };
            }

            public FeatureDescriptor Build()
            {
                _featureDescriptor.Dependencies = _featureDescriptor.Dependencies.Distinct().ToList();
                return _featureDescriptor;
            }

            public FeatureItem WithDescription(string desc)
            {
                _featureDescriptor.Description = desc;
                return this;
            }

            public FeatureItem WithCategory(string category)
            {
                _featureDescriptor.Category = category;
                return this;
            }

            public FeatureItem WithDependencies(params string[] name)
            {
                _featureDescriptor.Dependencies.AddRange(name);
                return this;
            }
        }
    }

    public struct FeatureDescriptor
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public List<string> Dependencies { get; set; }
    }

    public struct FeatureItem
    {
        public FeatureDescriptor FeatureDescriptor { get; set; }
        public FeatureState FeatureState { get; set; }
    }

    public enum FeatureState
    {
        Enabled, Disabled
    }
}
