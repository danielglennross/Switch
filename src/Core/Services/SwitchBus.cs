using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac.Features.Indexed;
using Castle.DynamicProxy;
using Core.Models;
using Core.Services.Cache;

namespace Core.Services
{
    public class SwitchBus : ISwitchBus // swap ISwitxh w/ IFeature
    {
        private readonly IIndex<string, IEnumerable<IFeature>> _features;
        private readonly IFeatureActionService _featureService;
        private readonly IInterceptCache _cache;

        public SwitchBus(IIndex<string, IEnumerable<IFeature>> features, IFeatureActionService featureService, IInterceptCache cache)
        {
            _cache = cache;
            _featureService = featureService;
            _features = features;
        }

        public async Task<T> Notify<T>(string messageName, IDictionary<string, object> eventData)
        {
            var parameters = messageName.Split('.');
            if (parameters.Length != 2)
            {
                throw new FormatException($"{nameof(messageName)} is not formatted correctly.");
            }
            var interfaceName = parameters[0];
            var methodName = parameters[1];

            var features = _features[interfaceName]
                .Select(x => ((IProxyTargetAccessor)x).DynProxyGetTarget())
                .ToList();

            var interfaceType = features.First().GetType().GetInterface(interfaceName);
            var crete =
                await _cache.Get(_cache.GetSwitchCacheKey(interfaceName),
                    async () => await _featureService.GetConcreteForInterface(features.Select(x => (IFeature) x))
                        .ConfigureAwait(false))
                            .ConfigureAwait(false);

            var method = GetMatchingMethod(interfaceType, methodName, eventData);

            dynamic result = method.Invoke(crete, method.GetParameters().Select(p => eventData[p.Name]).ToArray());

            // use the run time type to determine which overload to use
            return Handle(result ?? 0);
        }

        private static async Task Handle(Task task)
        {
            await task.ConfigureAwait(false);
        }

        private static async Task<T> Handle<T>(Task<T> task)
        {
            return await task.ConfigureAwait(false);
        }

        private static T Handle<T>(T result)
        {
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
