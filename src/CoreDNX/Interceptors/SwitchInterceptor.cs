using System.Linq;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using CoreDNX.Services;

namespace CoreDNX.Interceptors
{
    public class SwitchInterceptor : IInterceptor
    {
        private readonly ISwitchBus _switchBus;

        public SwitchInterceptor(ISwitchBus switchBus)
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

            var task = _switchBus.Notify(interfaceName + "." + methodName, data);
            task.ContinueWith(context =>
            {
                invocation.ReturnValue = context.Result;
            });
        }
    }
}
