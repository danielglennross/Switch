using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreDNX.Attributes
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
