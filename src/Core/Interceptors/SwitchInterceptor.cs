﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Core.Services;

namespace Core.Interceptors
{
    public class SwitchInterceptor : IInterceptor
    {
        class TaskCompletionSourceMethodMarkerAttribute : Attribute { }

        private readonly ISwitchBus _switchBus;

        private static readonly MethodInfo TaskCompletionSourceMethod = typeof(SwitchInterceptor)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(x => x.GetCustomAttributes(typeof(TaskCompletionSourceMethodMarkerAttribute)).Any());

        public SwitchInterceptor(ISwitchBus switchBus)
        {
            _switchBus = switchBus;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!typeof(Task).IsAssignableFrom(invocation.Method.ReturnType))
            {
                invocation.ReturnValue = InvokeActiveFeature(invocation).GetAwaiter().GetResult();
                return;
            }

            if (!invocation.Method.ReturnType.IsGenericType)
            {
                invocation.ReturnValue = TaskCompletionSource(invocation);
                return;
            }

            var returnType = invocation.Method.ReturnType.GetGenericArguments()[0];
            var method = TaskCompletionSourceMethod.MakeGenericMethod(returnType);
            invocation.ReturnValue = method.Invoke(this, new object[] { invocation });
        }

        private async Task<dynamic> InvokeActiveFeature(IInvocation invocation)
        {
            var interfaceName = invocation.Method.DeclaringType.Name;
            var methodName = invocation.Method.Name;

            var data = invocation.Method.GetParameters()
                .Select((parameter, index) => new { parameter.Name, Value = invocation.Arguments[index] })
                .ToDictionary(kv => kv.Name, kv => kv.Value);

            // 'T can be Task<>
            return await _switchBus.Notify<dynamic>(interfaceName + "." + methodName, data).ConfigureAwait(false);
        }

        private async Task TaskCompletionSource(IInvocation invocation)
        {
            dynamic nestedTask = await InvokeActiveFeature(invocation).ConfigureAwait(false);
            await ((Task)nestedTask).ConfigureAwait(false);
        }

        [TaskCompletionSourceMethodMarker]
        private async Task<TResult> TaskCompletionSource<TResult>(IInvocation invocation)
        {
            dynamic nestedTask = await InvokeActiveFeature(invocation).ConfigureAwait(false);
            var result = await ((Task<TResult>)nestedTask).ConfigureAwait(false);
            return result;
        }
    }
}
