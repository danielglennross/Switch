using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Core.Services;

namespace Core.Interceptors
{
    public class SwitchInterceptor : IInterceptor
    {
        private class TaskCompletionSourceMethodMarkerAttribute : Attribute { }

        private static readonly MethodInfo TaskCompletionSourceMethod = typeof(SwitchInterceptor)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
            .Single(x => x.GetCustomAttributes(typeof(TaskCompletionSourceMethodMarkerAttribute)).Any());

        private readonly ISwitchBus _switchBus;

        public SwitchInterceptor(ISwitchBus switchBus)
        {
            _switchBus = switchBus;
        }

        public void Intercept(IInvocation invocation)
        {
            if (!typeof(Task).IsAssignableFrom(invocation.Method.ReturnType))
            {
                invocation.ReturnValue = DoWork(invocation).GetAwaiter().GetResult();
                return;
            }

            var returnType = invocation.Method.ReturnType.IsGenericType ? invocation.Method.ReturnType.GetGenericArguments()[0] : typeof(object);
            var method = TaskCompletionSourceMethod.MakeGenericMethod(returnType);
            invocation.ReturnValue = method.Invoke(this, new object[] { invocation });
        }

        public Task<dynamic> DoWork(IInvocation invocation)
        {
            var interfaceName = invocation.Method.DeclaringType.Name;
            var methodName = invocation.Method.Name;

            var data = invocation.Method.GetParameters()
                .Select((parameter, index) => new { parameter.Name, Value = invocation.Arguments[index] })
                .ToDictionary(kv => kv.Name, kv => kv.Value);

            return _switchBus.Notify<dynamic>(interfaceName + "." + methodName, data);
        }

        [TaskCompletionSourceMethodMarker]
        private Task<TResult> TaskCompletionSource<TResult>(IInvocation invocation)
        {
            var tcs = new TaskCompletionSource<TResult>();

            var task2 = (Task) DoWork(invocation);

            var tcs2 = new TaskCompletionSource<object>();
            task2.ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    tcs2.SetException(x.Exception);
                    return;
                }

                dynamic dynamicTask = ((dynamic)task2).Result;
                object result = dynamicTask.Result;
                tcs2.SetResult(result);
            });

            var task = tcs2.Task;
            task.ContinueWith(x =>
            {
                if (x.IsFaulted)
                {
                    tcs.SetException(x.Exception);
                    return;
                }

                tcs.SetResult((TResult)x.Result);
            });

            return tcs.Task;
        }
    }

    //public class SwitchInterceptor : IInterceptor
    //{
    //    private readonly ISwitchBus _switchBus;

    //    public SwitchInterceptor(ISwitchBus switchBus)
    //    {
    //        _switchBus = switchBus;
    //    }

    //    public void Intercept(IInvocation invocation)
    //    {
    //        var interfaceName = invocation.Method.DeclaringType.Name;
    //        var methodName = invocation.Method.Name;

    //        var data = invocation.Method.GetParameters()
    //            .Select((parameter, index) => new { parameter.Name, Value = invocation.Arguments[index] })
    //            .ToDictionary(kv => kv.Name, kv => kv.Value);

    //        if (!IsAsyncMethod(invocation.Method))
    //        {
    //            var result = 
    //                _switchBus.Notify<dynamic>(interfaceName + "." + methodName, data)
    //                    .GetAwaiter().GetResult();
    //            invocation.ReturnValue = result;
    //        }
    //        else
    //        {
    //            var task = _switchBus.Notify<dynamic>(interfaceName + "." + methodName, data);

    //            //var test = invocation.Method.ReturnType.GetGenericArguments()[0];
    //            //dynamic vv = (dynamic)Activator.CreateInstance(test);

    //            //var tt = new TaskCompletionSource<dynamic>();
    //            //tt.SetResult((dynamic)vv);
    //            //var task4 = tt.Task;

    //            //var task2 = new Task<dynamic>(() => (int)vv);
    //            //var task3 = new Task<int>(() => (int)vv);

    //            var d1 = typeof(Task<>);
    //            Type[] typeArgs = { invocation.Method.ReturnType.GetGenericArguments()[0] };
    //            var makeme = d1.MakeGenericType(typeArgs);
    //            var o = (dynamic)Activator.CreateInstance(makeme, BindingFlags.CreateInstance);

    //            invocation.ReturnValue = o;

    //            task.ContinueWith(context =>
    //            {
    //                //invocation.ReturnValue = context.Result;
    //            });
    //        }
    //    }

    //    private static bool IsAsyncMethod(MethodInfo method)
    //    {
    //        return
    //            method.ReturnType == typeof(Task) || 
    //            (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>));
    //    }
    //}
}
