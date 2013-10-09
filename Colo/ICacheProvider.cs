using System;
using System.Diagnostics.CodeAnalysis;

namespace Colo
{
    /// <summary>
    /// Contract for Cache Providers
    /// </summary>
    public interface ICacheProvider
    {
        /// <summary>
        /// Retrieve and item from cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="type"></param>
        /// <returns></returns>
		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Get")]
		T Get<T>(string key, Type type = null) where T : class;
		
        /// <summary>
        /// Add an item to cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
		[SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Set")]
		void Set<T>(string key, T data, Type type = null) where T : class;
        
        /// <summary>
        /// Does and item exist in the cache
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
		bool IsSet(string key);
        /// <summary>
        /// Remove and Item from cache
        /// </summary>
        /// <param name="key"></param>
        void Invalidate(string key);
        /// <summary>
        /// Creates a name for the cache
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        string GenerateCacheName(string data);
        /// <summary>
        /// Get the cache item if found, otherwise execute and add to cache
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="execute"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        T GetOrExecuteAndAdd<T>(string key, Func<T> execute, Type type = null) where T : class;
    }
}
