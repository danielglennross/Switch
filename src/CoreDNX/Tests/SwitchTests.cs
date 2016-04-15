using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using CoreDNX.Autofac;
using CoreDNX.Services;
using NUnit.Framework;

namespace CoreDNX.Tests
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
                var testFeature = scope.Resolve<ITestFeature>();

                var r = testFeature.Run();
                testFeature.Do();

                var col = scope.Resolve<IEnumerable<ITestFeature>>();

                foreach (var feature in col)
                {
                    r = feature.Run();
                }
            }
        }
    }
}
