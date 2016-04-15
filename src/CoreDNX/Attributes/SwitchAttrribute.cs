using System;
using CoreDNX.FeatureRules;

namespace CoreDNX.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    class SwitchAttrribute : Attribute
    {
        public int Priority { get; }
        public Type Rule { get; }
        public string Group { get; }

        public SwitchAttrribute(int priority, Type ruleType = null, string group = null)
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
