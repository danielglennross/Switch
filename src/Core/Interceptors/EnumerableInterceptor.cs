using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Core.Models;
using Core.Services;
using Core.Services.Cache;

namespace Core.Interceptors
{
    public class EnumerableInterceptor : IInterceptor
    {
        private class FilterEnumerableMethodMarkerAttribute : Attribute { }

        private static readonly ConcurrentDictionary<Type, MethodInfo> EnumerableOfTypeTDictionary = new ConcurrentDictionary<Type, MethodInfo>();

        private static readonly MethodInfo FilterEnumerableMethod = typeof(EnumerableInterceptor)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(x => x.GetCustomAttributes(typeof(FilterEnumerableMethodMarkerAttribute)).Any());

        private readonly IFeatureActionService _featureService;
        private readonly IInterceptCache _cache;

        public EnumerableInterceptor(IFeatureActionService featureService, IInterceptCache cache)
        {
            _cache = cache;
            _featureService = featureService;
        }

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType.GetGenericArguments()[0];

            var method = EnumerableOfTypeTDictionary.GetOrAdd(returnType,
                type => FilterEnumerableMethod.MakeGenericMethod(returnType));

            method.Invoke(this, new object[] { invocation });

            invocation.Proceed();
        }

        [FilterEnumerableMethodMarker]
        private void FilterEnumerable<T>(IInvocation invocation)
        {
            var target = (Collection<T>)invocation.InvocationTarget;
            var features = target
                .Select(x => (T)((IProxyTargetAccessor)x).DynProxyGetTarget())
                .ToList();

            for (var i = 0; i < target.Count; i++)
            {
                target[i] = features[i];
            }

            var result = _cache.Get(_cache.GetEnumerableCacheKey(invocation.Method.DeclaringType.Name), 
                () => _featureService.FilterEnabledFeatures(features.Select(x => (IFeature) x)))
                    .GetAwaiter().GetResult();

            var items = features.Intersect(result.Select(x => (T)x));

            var targetCopy = target.ToList();
            foreach (var x in targetCopy.Where(x => !items.Contains(x)))
            {
                target.Remove(x);
            }
        }

        //public void Intercept(IInvocation invocation)
        //{
        //    var target = invocation.InvocationTarget;
        //    var features = ((IEnumerable<IFeature>)target)
        //        .Select(x => ((IProxyTargetAccessor)x).DynProxyGetTarget())
        //        .ToList();

        //    var result =
        //        _featureService.FilterEnabledFeatures(features.Select(x => (IFeature)x))
        //            .GetAwaiter().GetResult();

        //    var itemsToRemove = features.Except(result);
        //    var idexesToSkip = itemsToRemove
        //        .Select(x => features.Select((value, index) => new { value, index })
        //        .Single(p => p.value == x).index)
        //        .ToList();

        //    var count = ((dynamic)target).Count;
        //    for (var i = 0; i < count; i++)
        //    {
        //        ((dynamic)target)[i] = (dynamic)features[i]; //dynamic needed here to infer the correct runtime type
        //    }

        //    for (var i = 0; i < count; i++)
        //    {
        //        if (!idexesToSkip.Contains(i)) continue;
        //        ((dynamic)target).RemoveAt(i);
        //        --i;
        //        --count;
        //        idexesToSkip = idexesToSkip.Select(j => --j).ToList();
        //    }

        //    invocation.Proceed();
        //}
    }
}
