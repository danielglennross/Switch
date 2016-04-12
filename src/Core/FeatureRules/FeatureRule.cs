using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Core.FeatureRules
{
    public interface IRule
    {
        bool Evaluate();
    }

    public class Always : IRule
    {
        public bool Evaluate()
        {
            return true;
        }
    }

    public class IsVerison1 : IRule
    {
        public bool Evaluate()
        {
            return false;
        }
    }

    public class IsVerison2 : IRule
    {
        public bool Evaluate()
        {
            return true;
        }
    }
}
