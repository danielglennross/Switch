namespace Core.FeatureRules
{
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
