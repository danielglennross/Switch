using System.Collections.Generic;
using Autofac;
using CoreDNX.Autofac;
using static CoreDNX.TestFeatures;

namespace TestConsole
{
    public class Program
    {
        public static void Main(string[] args)
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
