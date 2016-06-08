using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.Providers;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Core.Providers
{
    public class RedisCacheFeatureProvider : IFeatureProvider
    {
        private const string EnabledFeaturesKey = "ISwitch.EnabledFeaturesKey";
        private readonly IFeatureProvider _decoratedFeatureProvider;

        public RedisCacheFeatureProvider(IFeatureProvider decoratedFeatureProvider)
        {
            _decoratedFeatureProvider = decoratedFeatureProvider;
        }

        public async Task<IEnumerable<string>> GetEnabledFeatures()
        {
            return
                await
                    Get(EnabledFeaturesKey, () => _decoratedFeatureProvider.GetEnabledFeatures())
                        .ConfigureAwait(false);
        }

        public async Task EnableFeature(string feature)
        {
            await 
                Delete(EnabledFeaturesKey, 
                    () => _decoratedFeatureProvider.EnableFeature(feature)).ConfigureAwait(false);
        }

        public async Task DisableFeature(string feature)
        {
            await
                Delete(EnabledFeaturesKey, 
                    () => _decoratedFeatureProvider.DisableFeature(feature)).ConfigureAwait(false);
        }

        private static async Task Delete(string key, Func<Task> decoratedAction)
        {
            var db = RedisConnectorHelper.Connection.GetDatabase();
            await decoratedAction().ConfigureAwait(false);
            await db.KeyDeleteAsync(key).ConfigureAwait(false);
        } 

        private static async Task<T> Get<T>(string key, Func<Task<T>> factory)
        {
            var db = RedisConnectorHelper.Connection.GetDatabase();
            var serialzedFeatures = await db.StringGetAsync(key).ConfigureAwait(false);
            if (serialzedFeatures.HasValue)
            {
                return JsonConvert.DeserializeObject<T>(serialzedFeatures.ToString());
            }

            var items = await factory().ConfigureAwait(false);
            await db.StringSetAsync(key, JsonConvert.SerializeObject(items)).ConfigureAwait(false);
            return items;
        } 
    }

    internal static class RedisConnectorHelper
    {
        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;
        static RedisConnectorHelper()
        {
            LazyConnection = new Lazy<ConnectionMultiplexer>(
                () => ConnectionMultiplexer.Connect(GetConnectionString()));
        }
        public static ConnectionMultiplexer Connection => LazyConnection.Value;

        private static string GetConnectionString() => 
            (ConfigurationManager.ConnectionStrings["ISwitch.RedisProvider"]?.ConnectionString) ?? "localhost";
    }
}
