using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace CoreDNX.FeatureRules
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

    public class WillReturnFalse : IRule
    {
        public bool Evaluate()
        {
            return false;
        }
    }

    public class WillReturnTrue : IRule
    {
        public bool Evaluate()
        {
            return true;
        }
    }
}
