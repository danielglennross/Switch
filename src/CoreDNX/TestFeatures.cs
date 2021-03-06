﻿using System;
using System.Threading.Tasks;
using CoreDNX.Attributes;
using CoreDNX.FeatureRules;
using CoreDNX.Models;

namespace CoreDNX
{
    public class TestFeatures
    {
        public interface ITestFeature : IFeature
        {
            int Run();
            void Do();
            Task<int> RunAsync();
            Task DoAsync(); 
        }

        [Feature("test1feature")]
        public class Test1f : ITestFeature
        {
            public int Run() => 1;
            public Task<int> RunAsync() => Task.FromResult(1);
            public void Do() => Console.WriteLine("1");
            public /*async*/ Task DoAsync()
            {
                Console.WriteLine("1");
                //await Task.Delay(100);
                return Task.FromResult(0);
            }

            private int _i = 1;
        }

        [Feature("test2feature")]
        [SuppressType(typeof (Test1f))]
        public class Test2f : ITestFeature
        {
            public int Run() => 2;
            public Task<int> RunAsync() => Task.FromResult(2);
            public void Do() { Console.WriteLine("2"); }
            public /*async*/ Task DoAsync()
            {
                Console.WriteLine("2");
                //await Task.Delay(100);
                return Task.FromResult(0);
            }

            private int _i = 2;
        }

        [Feature("test3feature")]
        [SuppressType(typeof (Test2f), typeof (WillReturnFalse))]
        public class Test3f : ITestFeature
        {
            public int Run() => 3;
            public Task<int> RunAsync() => Task.FromResult(3);
            public void Do() { Console.WriteLine("3"); }
            public /*async*/ Task DoAsync()
            {
                Console.WriteLine("3");
                //await Task.Delay(100);
                return Task.FromResult(0);
            }

            private int _i = 3;
        }
    }
}
