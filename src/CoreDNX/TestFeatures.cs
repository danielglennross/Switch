using System;
using CoreDNX.Attributes;
using CoreDNX.FeatureRules;
using CoreDNX.Models;

namespace CoreDNX
{
    public class TestFeatures
    {
        public interface ITestFeature : IFeature { int Run(); void Do(); }

        [Feature("test1feature")]
        public class Test1f : ITestFeature { public int Run() => 1; public void Do() { Console.WriteLine("1"); } private int _i = 1; }

        [Feature("test2feature")]
        [SuppressType(typeof(Test1f))]
        public class Test2f : ITestFeature { public int Run() => 2; public void Do() { Console.WriteLine("2"); } private int _i = 2; }

        [Feature("test3feature")]
        [SuppressType(typeof(Test2f), typeof(WillReturnFalse))]
        public class Test3f : ITestFeature { public int Run() => 3; public void Do() { Console.WriteLine("3"); } private int _i = 3; }
    }
}
