using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Autofac.Features.Indexed;
using Castle.DynamicProxy;
using Core.Autofac;
using Core.Models;
using Core.Services;

namespace Core.Interceptors
{
    public class SwitchIntercept : IInterceptor
    {
        private readonly ISwitchBus _switchBus;

        public SwitchIntercept(ISwitchBus switchBus)
        {
            _switchBus = switchBus;
        }

        public void Intercept(IInvocation invocation)
        {
            var interfaceName = invocation.Method.DeclaringType.Name;
            var methodName = invocation.Method.Name;

            var data = invocation.Method.GetParameters()
                .Select((parameter, index) => new { parameter.Name, Value = invocation.Arguments[index] })
                .ToDictionary(kv => kv.Name, kv => kv.Value);

            var result = _switchBus.Notify(interfaceName + "." + methodName, data);

            invocation.ReturnValue = result;
        }
    }

    public class CollectionIntercept : IInterceptor
    {
        private readonly IFeatureService _featureService;

        public CollectionIntercept(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        public void Intercept(IInvocation invocation)
        {
            var target = invocation.InvocationTarget;
            var features = ((IEnumerable<IFeature>)target).Select(x => ((IProxyTargetAccessor)x).DynProxyGetTarget()).ToList();

            var result = _featureService.FilterEnabledFeatures(features.Select(x => (IFeature)x));

            var itemsToRemove = features.Except(result);
            var idexesToSkip = itemsToRemove.Select(x => features.Select((value, index) => new { value, index }).Single(p => p.value == x).index);

            var count = ((dynamic)target).Count;
            for (var i = 0; i < count; i++)
            {
                ((dynamic)target)[i] = (dynamic)features[i]; //dynamic needed here to infer the correct runtime type
            }

            for (var i = 0; i < count; i++)
            {
                if (idexesToSkip.Contains(i))
                {
                    ((dynamic)target).RemoveAt(i);
                }
            }

            invocation.Proceed();
        }
    }
}
