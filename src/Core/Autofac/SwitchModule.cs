using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Autofac.Extras.DynamicProxy2;
using Autofac.Features.Indexed;
using Core.FeatureRules;
using Core.Interceptors;
using Core.Models;
using Core.Providers;
using Core.Services;
using Core.Startup;
using Module = Autofac.Module;

namespace Core.Autofac
{
    public class SwitchModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            builder.RegisterType<EnumerableInterceptor>().AsSelf();
            builder.RegisterType<SwitchInterceptor>().AsSelf();

            //test
            builder.RegisterType<TestFeatureManifest>().As<IFeatureManifest>();

            builder.RegisterType<FeatureManager>().As<IFeatureManager>().SingleInstance();
            builder.RegisterType<DefaultFeatureProvider>().As<IFeatureProvider>().SingleInstance();

            builder.RegisterType<FeatureInfoService>().As<IFeatureInfoService>();

            builder.RegisterType<FeatureActionService>().As<IFeatureActionService>();
            builder.RegisterType<SwitchBus>().As<ISwitchBus>();

            builder.Register<Func<Type, IRule>>(c =>
            {
                var indexedRules = c.Resolve<IIndex<Type, IRule>>();
                return a => indexedRules[a];
            });

            Func<Type, IEnumerable<Type>> getTypesForInterface = t => assemblies
                .SelectMany(s => s.GetTypes())
                .Where(p => t.IsAssignableFrom(p) && !p.IsInterface);

            // only register direct type that implements IRule (if this type implements any other interfaces, they must be explicit
            foreach (var t in getTypesForInterface(typeof (IRule)))
            {
                builder.RegisterType(t).Keyed<IRule>(t);
            }

            // only register direct type that implements IFeature (if this type implements any other interfaces, they must be explicit
            foreach (var t in getTypesForInterface(typeof (IFeature)))
            {
                foreach (var i in t.GetInterfaces()
                    .Where(x => typeof (IFeature).IsAssignableFrom(x) && x != typeof (IFeature)))
                {
                    builder.RegisterType(t).As(i)
                        .Named<IFeature>(i.Name)
                        .EnableInterfaceInterceptors()
                        .InterceptedBy(typeof (SwitchInterceptor));

                    builder.RegisterType(typeof (Collection<>).MakeGenericType(i))
                        .As(typeof (IEnumerable<>).MakeGenericType(i))
                        .EnableInterfaceInterceptors()
                        .InterceptedBy(typeof (EnumerableInterceptor));
                }
            }
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
}