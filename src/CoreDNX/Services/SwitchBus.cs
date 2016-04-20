using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Castle.DynamicProxy;
using CoreDNX.Helpers;
using CoreDNX.Models;

namespace CoreDNX.Services
{
    public interface ISwitchBus
    {
        object Notify(string messageName, IDictionary<string, object> eventData);
    }

    public class SwitchBus : ISwitchBus // swap ISwitxh w/ IFeature
    {
        private readonly IIndex<string, IEnumerable<IFeature>> _features;
        private readonly IFeatureActionService _featureService;

        public SwitchBus(IIndex<string, IEnumerable<IFeature>> features, IFeatureActionService featureService)
        {
            _featureService = featureService;
            _features = features;
        }

        public object Notify(string messageName, IDictionary<string, object> eventData)
        {
            var parameters = messageName.Split('.');
            if (parameters.Length != 2)
            {
                throw new FormatException($"{nameof(messageName)} is not formatted correctly.");
            }
            var interfaceName = parameters[0];
            var methodName = parameters[1];

            var features = _features[interfaceName].Select(x => ((IProxyTargetAccessor)x).DynProxyGetTarget());

            var interfaceType = features.First().GetType().GetInterface(interfaceName);
            var crete = _featureService.GetConcreteForInterface(features.Select(x => (IFeature)x));

            var method = GetMatchingMethod(interfaceType, methodName, eventData);

            var result = method.Invoke(crete, method.GetParameters().Select(p => eventData[p.Name]).ToArray());

            //var delegateCrete = DelegateHelper.CreateDelegate<IFeature>(crete.GetType(), method);

            //var args = method.GetParameters().Select(p => eventData[p.Name]).ToArray();

            //var result = delegateCrete(crete as IFeature, args);

            return result;
        }

        private static MethodInfo GetMatchingMethod(Type interfaceType, string methodName, IDictionary<string, object> arguments)
        {
            var allMethods = new List<MethodInfo>(interfaceType.GetMethods());
            var candidates = new List<MethodInfo>(allMethods);

            foreach (var method in allMethods)
            {
                if (string.Equals(method.Name, methodName, StringComparison.OrdinalIgnoreCase))
                {
                    var parameterInfos = method.GetParameters();
                    if (parameterInfos.Any(parameter => !arguments.ContainsKey(parameter.Name)))
                    {
                        candidates.Remove(method);
                    }
                }
                else
                {
                    candidates.Remove(method);
                }
            }

            // treating common case separately
            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            if (candidates.Count != 0)
            {
                return candidates.OrderBy(x => x.GetParameters().Length).Last();
            }

            return null;
        }
    }
}
