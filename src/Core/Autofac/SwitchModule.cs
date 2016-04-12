using System;
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
using Autofac.Extras.DynamicProxy2;
using Autofac.Features.Indexed;
using Autofac.Features.Scanning;
using Castle.DynamicProxy;
using Core.Attributes;
using Core.FeatureRules;
using Core.Interceptors;
using Core.Models;
using Core.Providers;
using Core.Services;
using Module = Autofac.Module;

namespace Core.Autofac
{
    public class SwitchModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            builder.RegisterType<CollectionIntercept>().AsSelf();
            builder.RegisterType<SwitchIntercept>().AsSelf();
            builder.RegisterType<DefaultFeatureProvider>().As<IFeatureProvider>();
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

            // only register direct type that implements IFeature (if this type implements any other interfaces, they must be explicit
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

    public interface ITestFeature : IFeature { int Run(); void Do(); }

    [FeatureAttrribute(1)]
    public class Test1 : ITestFeature { public int Run() => 1; public void Do() { Console.WriteLine("1"); } private int _i = 1; }

    [FeatureAttrribute(2, typeof(IsVerison1))]
    public class Test2 : ITestFeature { public int Run() => 2; public void Do() { Console.WriteLine("2"); } private int _i = 2; }

    [FeatureAttrribute(2, typeof(IsVerison2))]
    public class Test3 : ITestFeature { public int Run() => 3; public void Do() { Console.WriteLine("3"); } private int _i = 3; }


    public class EventsRegistrationSource : IRegistrationSource
    {
        private readonly DefaultProxyBuilder _proxyBuilder;

        public EventsRegistrationSource()
        {
            _proxyBuilder = new DefaultProxyBuilder();
        }

        public bool IsAdapterForIndividualComponents
        {
            get { return false; }
        }

        public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<IComponentRegistration>> registrationAccessor)
        {
            var serviceWithType = service as IServiceWithType;
            if (serviceWithType == null)
                yield break;

            var serviceType = serviceWithType.ServiceType;
            if (!serviceType.IsInterface || !typeof(IFeature).IsAssignableFrom(serviceType) || serviceType == typeof(IFeature))
                yield break;

            var interfaceProxyType = _proxyBuilder.CreateInterfaceProxyTypeWithoutTarget(
                serviceType,
                new Type[0],
                ProxyGenerationOptions.Default);


            var rb = RegistrationBuilder
                .ForDelegate((ctx, parameters) =>
                {
                    var interceptors = new IInterceptor[] {new SwitchIntercept(ctx.Resolve<ISwitchBus>())};
                    var args = new object[] {interceptors, null};
                    return Activator.CreateInstance(interfaceProxyType, args);
                })
                .As(service);

            yield return rb.CreateRegistration();
        }
    }

    public class DynamicProxyContext
    {
        const string ProxyContextKey = "Orchard.Environment.AutofacUtil.DynamicProxy2.DynamicProxyContext.ProxyContextKey";
        const string InterceptorServicesKey = "Orchard.Environment.AutofacUtil.DynamicProxy2.DynamicProxyContext.InterceptorServicesKey";

        readonly IProxyBuilder _proxyBuilder = new DefaultProxyBuilder();
        readonly IDictionary<Type, Type> _cache = new Dictionary<Type, Type>();

        /// <summary>
        /// Static method to resolve the context for a component registration. The context is set
        /// by using the registration builder extension method EnableDynamicProxy(context).
        /// </summary>
        public static DynamicProxyContext From(IComponentRegistration registration)
        {
            object value;
            if (registration.Metadata.TryGetValue(ProxyContextKey, out value))
                return value as DynamicProxyContext;
            return null;
        }

        /// <summary>
        /// Called indirectly from the EnableDynamicProxy extension method.
        /// Modifies a registration to support dynamic interception if needed, and act as a normal type otherwise.
        /// </summary>
        public void EnableDynamicProxy<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> registrationBuilder)
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
        {

            // associate this context. used later by static DynamicProxyContext.From() method.
            registrationBuilder.WithMetadata(ProxyContextKey, this);

            // put a shim in place. this will return constructors for the proxy class if it interceptors have been added.
            registrationBuilder.ActivatorData.ConstructorFinder = new ConstructorFinderWrapper(
                registrationBuilder.ActivatorData.ConstructorFinder, this);

            // when component is being resolved, this even handler will place the array of appropriate interceptors as the first argument
            registrationBuilder.OnPreparing(e => {
                object value;
                if (e.Component.Metadata.TryGetValue(InterceptorServicesKey, out value))
                {
                    var interceptorServices = (IEnumerable<Service>)value;
                    var interceptors = interceptorServices.Select(service => e.Context.ResolveService(service)).Cast<IInterceptor>().ToArray();
                    var parameter = new PositionalParameter(0, interceptors);
                    e.Parameters = new[] { parameter }.Concat(e.Parameters).ToArray();
                }
            });
        }

        /// <summary>
        /// Called indirectly from the InterceptedBy extension method.
        /// Adds services to the componenent's list of interceptors, activating the need for dynamic proxy
        /// </summary>
        public void AddInterceptorService(IComponentRegistration registration, Service service)
        {
            AddProxy(registration.Activator.LimitType);

            var interceptorServices = Enumerable.Empty<Service>();
            object value;
            if (registration.Metadata.TryGetValue(InterceptorServicesKey, out value))
            {
                interceptorServices = (IEnumerable<Service>)value;
            }

            registration.Metadata[InterceptorServicesKey] = interceptorServices.Concat(new[] { service }).Distinct().ToArray();
        }


        /// <summary>
        /// Ensures that a proxy has been generated for the particular type in this context
        /// </summary>
        public void AddProxy(Type type)
        {
            Type proxyType;
            if (_cache.TryGetValue(type, out proxyType))
                return;

            lock (_cache)
            {
                if (_cache.TryGetValue(type, out proxyType))
                    return;

                _cache[type] = _proxyBuilder.CreateClassProxyType(type, new Type[0], ProxyGenerationOptions.Default);
            }
        }

        /// <summary>
        /// Determines if a proxy has been generated for the given type, and returns it.
        /// </summary>
        public bool TryGetProxy(Type type, out Type proxyType)
        {
            return _cache.TryGetValue(type, out proxyType);
        }

    }

    class ConstructorFinderWrapper : IConstructorFinder
    {
        private readonly IConstructorFinder _constructorFinder;
        private readonly DynamicProxyContext _dynamicProxyContext;

        public ConstructorFinderWrapper(IConstructorFinder constructorFinder, DynamicProxyContext dynamicProxyContext)
        {
            _constructorFinder = constructorFinder;
            _dynamicProxyContext = dynamicProxyContext;
        }

        public ConstructorInfo[] FindConstructors(Type targetType)
        {
            Type proxyType;
            if (_dynamicProxyContext.TryGetProxy(targetType, out proxyType))
            {
                return _constructorFinder.FindConstructors(proxyType);
            }
            return _constructorFinder.FindConstructors(targetType);
        }
    }

    public static class DynamicProxyExtensions
    {

        public static IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> EnableDynamicProxy<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TConcreteReflectionActivatorData, TRegistrationStyle> rb,
            DynamicProxyContext dynamicProxyContext)
            where TConcreteReflectionActivatorData : ConcreteReflectionActivatorData
        {

            dynamicProxyContext.EnableDynamicProxy(rb);

            return rb;
        }

        public static IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> EnableDynamicProxy<TLimit, TRegistrationStyle>(
           this IRegistrationBuilder<TLimit, ScanningActivatorData, TRegistrationStyle> rb,
            DynamicProxyContext dynamicProxyContext)
        {

            rb.ActivatorData.ConfigurationActions.Add((t, rb2) => rb2.EnableDynamicProxy(dynamicProxyContext));
            return rb;
        }

        public static void InterceptedBy<TService>(this IComponentRegistration cr)
        {
            var dynamicProxyContext = DynamicProxyContext.From(cr);
            if (dynamicProxyContext == null)
                throw new ApplicationException(string.Format("Component {0} was not registered with EnableDynamicProxy", cr.Activator.LimitType));

            dynamicProxyContext.AddInterceptorService(cr, new TypedService(typeof(TService)));
        }
    }
}
