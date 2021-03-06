﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autofac;
using Autofac.Features.Indexed;
using Autofac.Extras.DynamicProxy;
using CoreDNX.FeatureRules;
using CoreDNX.Interceptors;
using CoreDNX.Models;
using CoreDNX.Providers;
using CoreDNX.Services;
using CoreDNX.Services.Cache;
using CoreDNX.Services.Events;
using CoreDNX.Startup;
using Module = Autofac.Module;

namespace CoreDNX.Autofac
{
    public class SwitchModule : Module
    {
        public bool UseRedisCacheProvider { get; set; }

        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            builder.RegisterType<EnumerableInterceptor>().AsSelf();
            builder.RegisterType<SwitchInterceptor>().AsSelf();

            //test
            builder.RegisterType<TestFeatureManifest>().As<IFeatureManifest>();

            //if (UseRedisCacheProvider)
            //{
            //    const string key = "featureProvider";
            //    builder.RegisterType<DefaultFeatureProvider>().Named<IFeatureProvider>(key).SingleInstance();
            //    builder.RegisterDecorator<RedisCacheFeatureProvider>(
            //        (context, provider) => new RedisCacheFeatureProvider(provider), key).SingleInstance();
            //}
            //else
            //{
            //    builder.RegisterType<DefaultFeatureProvider>().As<IFeatureProvider>().SingleInstance();
            //}

            builder.RegisterType<CacheInvalidatorEvent>().As<ICacheEvents>();

            builder.RegisterType<DefaultObjectCache>().As<IInterceptCache>().SingleInstance();
            builder.RegisterType<FeatureManager>().As<IFeatureManager>().SingleInstance();
            builder.RegisterType<DefaultFeatureProvider>().As<IFeatureProvider>().SingleInstance();

            builder.RegisterType<FeatureInfoService>().As<IFeatureInfoService>();

            builder.RegisterType<FeatureActionService>().As<IFeatureActionService>().OnActivated(act =>
            {
                var handlers = act.Context.Resolve<IEnumerable<ICacheEvents>>().ToList();
                handlers.ForEach(h =>
                {
                    act.Instance.OnFeatureEnabled += h.OnFeatureEnabled;
                    act.Instance.OnFeatureDisabled += h.OnFeatureDisabled;
                });
            });


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