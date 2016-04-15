using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.FeatureRules;

namespace Core.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    class SuppressTypeAttribute : Attribute
    {
        public Type Type { get; }
        public Type RuleType { get; }

        public SuppressTypeAttribute(Type type, Type ruleType = null)
        {
            ruleType = ruleType ?? typeof(Always);
            if (!typeof(IRule).IsAssignableFrom(ruleType))
            {
                throw new ArgumentException($"{nameof(ruleType)} does implement IRule");
            }

            RuleType = ruleType;
            Type = type;
        }
    }
}
