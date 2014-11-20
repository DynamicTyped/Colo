using System;
using System.IO;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;
using Colo.Configuration;
using ProtoBuf;
using StackExchange.Redis;

namespace Colo.Redis
{
    /// <summary>
    /// Redis based cache provider (for use with services like Amazon ElastiCache and Microsoft Azure Cache)
    /// </summary>
    public class RedisCacheProvider : BaseCacheProvider
    {
        /// <summary>
        /// Not implemented
        /// </summary>
        protected override ObjectCache Cache
        {
            get { return null; }
        }

        private readonly IDatabase _cache;
        public RedisCacheProvider(ConnectionMultiplexer connection)
        {
            if (connection == null) throw new ArgumentNullException("connection");

            _cache = connection.GetDatabase();
        }

        /// <summary>
        /// Gets item from cached file
        /// T or Type must be a decorated with ProtoContract or this will not retrieve anything
        /// </summary>
        /// <typeparam name="T">Type of data to return</typeparam>
        /// <param name="key">name of the file</param>
        /// <param name="type">If T is a some form of enumeration, then type must be passed in.  It is the type of the enumeration. For example if T = List of int, type must be typeof(int).  If T is double or some other class type can be null</param>
        /// <returns></returns>
        public override T Get<T>(string key, Type type = null)
        {
            try
            {
                if (CachingSection.GetSection.UseCache)
                {
                    if (IsCacheable<T>(type) && IsSet(key))
                    {
                        return Deserialize<T>(_cache.StringGet(key));
                    }
                    return null;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves an object to the file based cache
        /// If T or type is not decorated with ProtoContract, cache will not save
        /// </summary>
        /// <typeparam name="T">type of the object to save</typeparam>
        /// <param name="key">filename</param>
        /// <param name="data">data to store</param>
        /// <param name="type">If T is a some form of enumeration, then type must be passed in.  It is the type of the enumeration. For example if T = List of int, type must be typeof(int).  If T is double or some other class type can be null</param>
        public override void Set<T>(string key, T data, Type type = null)
        {
            try
            {
                var cacheElement = GetCacheDefaults(data, type);
                if (CachingSection.GetSection.UseCache && cacheElement.Enabled)
                {
                    if (IsCacheable<T>(type))
                    {
                        var expiry = new TimeSpan(0, cacheElement.CacheLife, 0);
                        _cache.StringSet(key, Serialize(data), expiry);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }

        public override bool IsSet(string key)
        {
            return _cache.KeyExists(key);
        }

        public override void Invalidate(string key)
        {
            // Expire the key in a second
            _cache.KeyExpire(key, new TimeSpan(0, 0, 1));
        }

        public override string GenerateCacheName(string data)
        {
            using (var md5 = MD5.Create())
            {
                var hash = md5.ComputeHash(Encoding.Default.GetBytes(data));

                return BitConverter.ToString(hash);
            }
        }

        private static bool IsCacheable<T>(Type type = null)
        {
            return ((type != null && DoesTypeHaveContract(type)) ||
                    (type == null && DoesTypeHaveContract(typeof(T))));
        }

        /// <summary>
        /// Checks to ensure the type supports ProtoBuf by having the protocontract attribute on the class
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static bool DoesTypeHaveContract(Type type)
        {
            try
            {
                var hasContract = type.IsDefined(typeof(ProtoContractAttribute), false);

                if (!hasContract && !type.IsDefined(typeof(SuppressCachingWarningAttribute), false))
                {
                    System.Diagnostics.Trace.WriteLine(type.Name + " does not have protobuf attributes and can not be cached");
                }

                return hasContract;
            }
            catch
            {
                return false;
            }
        }

        static byte[] Serialize(object o)
        {
            if (o == null)
                return null;

            using (var memoryStream = new MemoryStream())
            {
                Serializer.Serialize(memoryStream, o);
                var objectDataAsStream = memoryStream.ToArray();
                return objectDataAsStream;
            }
        }

        static T Deserialize<T>(byte[] stream)
        {
            if (stream == null)
                return default(T);

            using (var memoryStream = new MemoryStream(stream))
            {
                var result = Serializer.Deserialize<T>(memoryStream);
                return result;
            }
        }
    }
}
