using System;
using Core.FeatureRules;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    class FeatureAttrribute : Attribute
    {
        public int Priority { get; }
        public Type Rule { get; }
        public string Group { get; }

        public FeatureAttrribute(int priority, Type ruleType = null, string group = null)
        {
            ruleType = ruleType ?? typeof (Always);
            if (!typeof (IRule).IsAssignableFrom(ruleType))
            {
                throw new ArgumentException($"{nameof(ruleType)} does implement IRule");
            }

            Priority = priority;
            Rule = ruleType;
            Group = group;
        }
    }
}
