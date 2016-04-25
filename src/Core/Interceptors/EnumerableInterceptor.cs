using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Castle.DynamicProxy;
using Core.Models;
using Core.Services;

namespace Core.Interceptors
{
    public class EnumerableInterceptor : IInterceptor
    {
        private readonly IFeatureActionService _featureService;

        public EnumerableInterceptor(IFeatureActionService featureService)
        {
            _featureService = featureService;
        }

        //public void Intercept(IInvocation invocation)
        //{
        //    var returnType = invocation.Method.ReturnType.GetGenericArguments()[0];
        //    var method = GetType().GetMethod("Get", BindingFlags.Instance | BindingFlags.NonPublic).MakeGenericMethod(returnType);

        //    var target = method.Invoke(this, new object[] { invocation });

        //    invocation.ReturnValue = target;
        //    invocation.Proceed();
        //}

        //private IEnumerable<T> Get<T>(IInvocation invocation)
        //{
        //    var target = invocation.InvocationTarget;
        //    var features = ((IEnumerable<T>)target)
        //        .Select(x => (T)((IProxyTargetAccessor)x).DynProxyGetTarget())
        //        .ToList();

        //    var result =
        //        _featureService.FilterEnabledFeatures(features.Select(x => (IFeature)x))
        //            .GetAwaiter().GetResult();

        //    var items = features.Intersect(result.Select(x => (T)x));
        //    return items;
        //} 

        public void Intercept(IInvocation invocation)
        {
            var target = invocation.InvocationTarget;
            var features = ((IEnumerable<IFeature>)target)
                .Select(x => ((IProxyTargetAccessor)x).DynProxyGetTarget())
                .ToList();

            var result =
                _featureService.FilterEnabledFeatures(features.Select(x => (IFeature)x))
                    .GetAwaiter().GetResult();

            var itemsToRemove = features.Except(result);
            var idexesToSkip = itemsToRemove
                .Select(x => features.Select((value, index) => new { value, index })
                .Single(p => p.value == x).index)
                .ToList();

            var count = ((dynamic)target).Count;
            for (var i = 0; i < count; i++)
            {
                ((dynamic)target)[i] = (dynamic)features[i]; //dynamic needed here to infer the correct runtime type
            }

            for (var i = 0; i < count; i++)
            {
                if (!idexesToSkip.Contains(i)) continue;
                ((dynamic)target).RemoveAt(i);
                --i;
                --count;
                idexesToSkip = idexesToSkip.Select(j => --j).ToList();
            }

            invocation.Proceed();
        }
    }
}
