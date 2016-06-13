//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Threading.Tasks;
//using CoreDNX.Models;
//using Newtonsoft.Json;
//using StackExchange.Redis;

//namespace CoreDNX.Services.Cache
//{
//    public class RedisDistributedObjectCache : IInterceptCache
//    {
//        public string GetEnumerableCacheKey(string interfaceName) => interfaceName + "_Enumerable_Redis";

//        public string GetSwitchCacheKey(string interfaceName) => interfaceName + "_Switch_Redis";

//        public async Task<T> Get<T>(string key, Func<Task<T>> factory)
//        {
//            var db = Providers.RedisConnectorHelper.Connection.GetDatabase();
//            var serialzedFeatures = await db.StringGetAsync(key).ConfigureAwait(false);
//            if (serialzedFeatures.HasValue)
//            {
//                return JsonConvert.DeserializeObject<T>(serialzedFeatures.ToString());
//            }

//            var items = await factory().ConfigureAwait(false);
//            await db.StringSetAsync(key, JsonConvert.SerializeObject(items)).ConfigureAwait(false);
//            return items;
//        }

//        public async Task Remove(string key)
//        {
//            var db = Providers.RedisConnectorHelper.Connection.GetDatabase();
//            await db.KeyDeleteAsync(key).ConfigureAwait(false);
//        }
//    }

//    internal static class RedisConnectorHelper
//    {
//        private static readonly Lazy<ConnectionMultiplexer> LazyConnection;
//        static RedisConnectorHelper()
//        {
//            LazyConnection = new Lazy<ConnectionMultiplexer>(
//                () => ConnectionMultiplexer.Connect(GetConnectionString()));
//        }
//        public static ConnectionMultiplexer Connection => LazyConnection.Value;

//        private static string GetConnectionString() =>
//            (ConfigurationManager.ConnectionStrings["ISwitch.RedisProvider"]?.ConnectionString) ?? "localhost";
//    }
//}
