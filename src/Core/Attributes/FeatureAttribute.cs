using System;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    class FeatureAttribute : Attribute
    {
        public string Name { get; }

        public FeatureAttribute(string name)
        {
            Name = name;
        }
    }
}
