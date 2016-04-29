using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
using Core.Attributes;

namespace Core.Startup
{
    public class FeatureBuilder
    {
        private readonly Lazy<IEnumerable<Type>> _loadedAssemblies; 
        private readonly IList<FeatureDescriptor> _features;

        public FeatureBuilder()
        {
            _features = new List<FeatureDescriptor>();
            _loadedAssemblies = new Lazy<IEnumerable<Type>>(() => AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes()));
        }

        public FeatureBuilder ForFeature(string name, Action<FeatureItemInternal> featureFactory)
        {
            var builder = new FeatureItemInternal(name, _loadedAssemblies);
            featureFactory(builder);

            _features.Add(builder.Build());
            return this;
        }
        public IEnumerable<FeatureDescriptor> Build()
        {
            //build proper connections
            return _features ?? Enumerable.Empty<FeatureDescriptor>();
        }

        public class FeatureItemInternal
        {
            private readonly List<string> _dependencies; 
            private readonly Lazy<IEnumerable<Type>> _lazy;
            private FeatureDescriptor _featureDescriptor;

            public FeatureItemInternal(string name, Lazy<IEnumerable<Type>> loadedAssemblies)
            {
                _lazy = loadedAssemblies;
                _dependencies = new List<string>();
                _featureDescriptor = new FeatureDescriptor
                {
                    Name = name,
                };
            }

            public FeatureDescriptor Build()
            {
                var exportedTypes = _lazy.Value
                    .Where(x => x.GetCustomAttributes(typeof (FeatureAttribute), true)
                    .Any(a => a != null && 
                        ((FeatureAttribute)a).Name.Equals(_featureDescriptor.Name, StringComparison.OrdinalIgnoreCase)));

                _featureDescriptor.ExportedTypes = exportedTypes;
                _featureDescriptor.Dependencies = _dependencies.Distinct();
                return _featureDescriptor;
            }

            public FeatureItemInternal WithDescription(string desc)
            {
                _featureDescriptor.Description = desc;
                return this;
            }

            public FeatureItemInternal WithCategory(string category)
            {
                _featureDescriptor.Category = category;
                return this;
            }

            public FeatureItemInternal WithDependencies(params string[] name)
            {
                _dependencies.AddRange(name);
                return this;
            }
        }
    }

    public struct FeatureDescriptor
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public IEnumerable<string> Dependencies { get; set; }
        public IEnumerable<Type> ExportedTypes { get; set; } 
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
