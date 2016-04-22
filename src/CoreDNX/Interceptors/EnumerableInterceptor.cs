using System.Collections.Generic;
using System.Linq;
using Castle.DynamicProxy;
using CoreDNX.Models;
using CoreDNX.Services;

namespace CoreDNX.Interceptors
{
    public class EnumerableInterceptor : IInterceptor
    {
        private readonly IFeatureActionService _featureService;

        public EnumerableInterceptor(IFeatureActionService featureService)
        {
            _featureService = featureService;
        }

        public void Intercept(IInvocation invocation)
        {
            var target = invocation.InvocationTarget;
            var features = ((IEnumerable<IFeature>)target)
                .Select(x => ((IProxyTargetAccessor)x).DynProxyGetTarget())
                .ToList();

            var task = 
                _featureService.FilterEnabledFeatures(features.Select(x => (IFeature)x));

            task.ContinueWith(context =>
            {
                var itemsToRemove = features.Except(context.Result);
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
            });
        }
    }
}
