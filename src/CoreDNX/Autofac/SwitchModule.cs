using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Builder;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Autofac.Extras.DynamicProxy;
using Autofac.Features.Indexed;
using Autofac.Features.Scanning;
using Castle.DynamicProxy;
using Castle.DynamicProxy.Internal;
using CoreDNX.Attributes;
using CoreDNX.FeatureRules;
using CoreDNX.Interceptors;
using CoreDNX.Models;
using CoreDNX.Providers;
using CoreDNX.Services;
using Module = Autofac.Module;

namespace CoreDNX.Autofac
{
    public class SwitchModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            builder.RegisterType<CollectionIntercept>().AsSelf();
            builder.RegisterType<SwitchIntercept>().AsSelf();
            builder.RegisterType<DefaultFeatureProvider>().As<IFeatureProvider>().SingleInstance();
            builder.RegisterType<FeatureService>().As<IFeatureService>();
            builder.RegisterType<SwitchBus>().As<ISwitchBus>();

            builder.Register<Func<Type, IRule>>(c => {
                var indexedRules = c.Resolve<IIndex<Type, IRule>>();
                return a => indexedRules[a];
            });

            Func<Type, IEnumerable<Type>> getTypesForInterface = t => assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => t.IsAssignableFrom(p) && !p.IsInterface);

            // only register direct type that implements IRule (if this type implements any other interfaces, they must be explicit
            foreach (var t in getTypesForInterface(typeof(IRule)))
            {
                builder.RegisterType(t).Keyed<IRule>(t);
            }

            // only register direct type that implements ISwitch (if this type implements any other interfaces, they must be explicit
            foreach (var t in getTypesForInterface(typeof(ISwitch)))
            {
                foreach (var i in t.GetInterfaces()
                    .Where(x => typeof(ISwitch).IsAssignableFrom(x) && x != typeof(ISwitch)))
                {
                    builder.RegisterType(t).As(i)
                        .Named<ISwitch>(i.Name)
                        .EnableInterfaceInterceptors()
                        .InterceptedBy(typeof(SwitchIntercept));

                    builder.RegisterType(typeof(Collection<>).MakeGenericType(i)).As(typeof(IEnumerable<>).MakeGenericType(i))
                        .EnableInterfaceInterceptors()
                        .InterceptedBy(typeof(CollectionIntercept));
                }
            }

            foreach (var t in getTypesForInterface(typeof(IFeature)))
            {
                foreach (var i in t.GetInterfaces()
                    .Where(x => typeof(IFeature).IsAssignableFrom(x) && x != typeof(IFeature)))
                {
                    builder.RegisterType(t).As(i)
                        .Named<IFeature>(i.Name)
                        .EnableInterfaceInterceptors()
                        .InterceptedBy(typeof(SwitchIntercept));

                    builder.RegisterType(typeof(Collection<>).MakeGenericType(i)).As(typeof(IEnumerable<>).MakeGenericType(i))
                        .EnableInterfaceInterceptors()
                        .InterceptedBy(typeof(CollectionIntercept));
                }
            }

            //builder.RegisterType(typeof(ReadOnlyCollection<ITestFeature>)).As(typeof(IEnumerable<ITestFeature>))
            //    .EnableInterfaceInterceptors()
            //    .InterceptedBy(typeof(CollectionIntercept));

            //builder.Register(c => c.Resolve<IEnumerable<ITestFeature>>())
            //    .EnableInterfaceInterceptors()
            //    .InterceptedBy(typeof (CollectionIntercept));

            //builder.RegisterType<IEnumerable<ITestFeature>>()
            //    .AsSelf()
            //    .EnableInterfaceInterceptors()
            //    .InterceptedBy(typeof (CollectionIntercept));
        }

        //protected override void Load(ContainerBuilder builder)
        //{

            //    builder.RegisterType<SwitchIntercept>().AsSelf();

            //    builder.RegisterType<TestClass>().As<ITestClass>();

            //    builder.RegisterType<Always>().Keyed<IRule>(typeof(Always));
            //    builder.RegisterType<IsVerison1>().Keyed<IRule>(typeof(IsVerison1));
            //    builder.RegisterType<IsVerison2>().Keyed<IRule>(typeof(IsVerison2));

            //    builder.Register<Func<Type, IRule>>(c => {
            //        var context = c.Resolve<IIndex<Type, IRule>>();
            //        return a => context[a];
            //    });

            //    builder.RegisterType<DefaultFeatureProvider>().As<IFeatureProvider>();
            //    builder.RegisterType<FeatureService>().As<IFeatureService>();
            //    builder.RegisterType<SwitchBus>().As<ISwitchBus>();

            //    builder.RegisterType<Test1>().As<ITestFeature>().Named<IFeature>("ITestFeature").EnableInterfaceInterceptors().InterceptedBy(typeof(SwitchIntercept));
            //    builder.RegisterType<Test2>().As<ITestFeature>().Named<IFeature>("ITestFeature").EnableInterfaceInterceptors().InterceptedBy(typeof(SwitchIntercept));
            //    builder.RegisterType<Test3>().As<ITestFeature>().Named<IFeature>("ITestFeature").EnableInterfaceInterceptors().InterceptedBy(typeof(SwitchIntercept));
            //}
        }

    public interface ITestSwitch : ISwitch { int Run(); void Do(); }

    [SwitchAttrribute(1)]
    public class Test1 : ITestSwitch { public int Run() => 1; public void Do() { Console.WriteLine("1"); } private int _i = 1; }

    [SwitchAttrribute(2, typeof(WillReturnFalse))]
    public class Test2 : ITestSwitch { public int Run() => 2; public void Do() { Console.WriteLine("2"); } private int _i = 2; }

    [SwitchAttrribute(2, typeof(WillReturnTrue))]
    public class Test3 : ITestSwitch { public int Run() => 3; public void Do() { Console.WriteLine("3"); } private int _i = 3; }

    public interface ITestFeature : IFeature { int Run(); void Do(); }

    [FeatureAttribute("test1feature")]
    public class Test1f : ITestFeature { public int Run() => 1; public void Do() { Console.WriteLine("1"); } private int _i = 1; }

    [FeatureAttribute("test2feature")]
    [SuppressType(typeof(Test1f))]
    public class Test2f : ITestFeature { public int Run() => 2; public void Do() { Console.WriteLine("2"); } private int _i = 2; }

    [FeatureAttribute("test3feature")]
    [SuppressType(typeof(Test2f), typeof(WillReturnFalse))]
    public class Test3f : ITestFeature { public int Run() => 3; public void Do() { Console.WriteLine("3"); } private int _i = 3; }

}
