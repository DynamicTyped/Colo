using System.Runtime.Caching;

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
    }
}
