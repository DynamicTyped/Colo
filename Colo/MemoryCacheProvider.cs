using System;
using System.Runtime.Caching;
using System.Threading;
using Colo.Configuration;

namespace Colo
{
    /// <summary>
    /// Basic memory based cache
    /// </summary>
    public class MemoryCacheProvider : BaseCacheProvider
    {
        protected override ObjectCache Cache
        {
            get { return MemoryCache.Default; }
        }

        public override void Set<T>(string key, T data, Type type = null)
        {
            try
            {
                var cacheElement = GetCacheDefaults(data, type);
                if (CachingSection.GetSection.UseCache && cacheElement.Enabled)
                {
                    Cache.Set(
                        key,
                        data,
                        new CacheItemPolicy
                        {
                            AbsoluteExpiration = DateTime.Now.AddMinutes(cacheElement.CacheLife)
                        });

                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
            }
        }

    }
}
