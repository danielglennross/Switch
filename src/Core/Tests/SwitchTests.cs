using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Core.Autofac;
using Core.Services;
using NUnit.Framework;
using static Core.TestFeatures;

namespace Core.Tests
{
    [TestFixture]
    public class SwitchTests
    {
        [Test]
        public void Test()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule(new SwitchModule());
            var container = builder.Build();

            using (var scope = container.BeginLifetimeScope())
            {
                var featureActionService = scope.Resolve<IFeatureActionService>();
                featureActionService.EnableFeature("test1feature").GetAwaiter().GetResult();
                featureActionService.EnableFeature("test2feature").GetAwaiter().GetResult();

                var testFeature = scope.Resolve<ITestFeature>();

                var r = testFeature.Run();
                var r2 = testFeature.Run();

                featureActionService.EnableFeature("test3feature").GetAwaiter().GetResult();

                var r3 = testFeature.Run();

                testFeature.Do();

                var col = scope.Resolve<IEnumerable<ITestFeature>>();

                foreach (var feature in col)
                {
                    r = feature.Run();
                }

                var rTask = testFeature.RunAsync();
                rTask.ContinueWith(x =>
                {
                    var rr = x.Result;
                });

                var rTask2 = testFeature.RunAsync();
                rTask2.ContinueWith(x =>
                {
                    var rr = x.Result;
                });

                //var dTask = testFeature.DoAsync();
                //dTask.ContinueWith(x =>
                //{
                //    var rr = x.Status;
                //});

                Thread.Sleep(Timeout.Infinite);
            }
        }
    }
}
