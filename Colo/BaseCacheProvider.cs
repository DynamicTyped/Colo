using Colo.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Text;

namespace Colo
{
	/// <summary>
	/// Base level caching object that all caching derives from
	/// </summary>
	public abstract class BaseCacheProvider : ICacheProvider
	{
		/// <summary>
		/// Main cache object
		/// </summary>
		protected abstract ObjectCache Cache { get; }        

		/// <summary>
		/// Removes all of the cache
		/// </summary>
		public virtual bool Clear()
		{
			var cacheItems = MemoryCache.Default.AsParallel().Select(c => c).ToList();

			foreach (var item in cacheItems)
			{
				MemoryCache.Default.Remove(item.Key);
			}

		    return true;
		}

		/// <summary>
		/// Determines if the key is found in the cache
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public virtual bool IsSet(string key)
		{
			return (Cache[key] != null);
		}

		/// <summary>
		/// Removes and item from the cache
		/// </summary>
		/// <param name="key"></param>
		public virtual void Invalidate(string key)
		{
			Cache.Remove(key);
		}

		/// <summary>
		/// Create a cache name based on the data
		/// </summary>
		/// <param name="data"></param>
		/// <returns>A MD5 hash of the data converted to string</returns>
		public virtual string GenerateCacheName(string data)
		{
			using (var md5 = MD5.Create())
			{
				var hash = md5.ComputeHash(Encoding.Default.GetBytes(data));

				return BitConverter.ToString(hash);
			}
		}

		/// <summary>
		/// Retrieves and item from the cache
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual T Get<T>(string key, Type type = null) where T : class
		{
			return (T)Cache[key];
		}

		/// <summary>
		/// Gets the caching rules for the passed in type
		/// Looks for rules in the order of type, T, and the system default
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="propertyType"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static CacheDefaultValueElement GetCacheDefaults<T>(T propertyType, Type type = null) where T : class
		{
			var cachingSection = CachingSection.GetSection;
			CacheDefaultValueElement cacheTimeByType = null;

            // First, iterate through the dependency tree for the "type" parameter.
            // Second, iterate through the dependency tree for type "T".
            // Next, if it's an enumerable type, iterate through it.
            var dependencyTree = WalkDependencyTree(type)
                .Union(WalkDependencyTree(typeof(T)))
                .Union(WalkDependencyTree(GetEnumerableType(typeof(T))));

		    var cacheName = dependencyTree.FirstOrDefault(cachingSection.CacheDefaults.ContainsKey);

            cacheTimeByType = cacheName == null ? null
                : (CacheDefaultValueElement) cachingSection.CacheDefaults[cacheName];

			// use the configuration time or the default
			return cacheTimeByType ??
				   new CacheDefaultValueElement {Enabled = cachingSection.UseCache, CacheLife = cachingSection.CacheLife};

		}

	    private static IEnumerable<string> WalkDependencyTree(Type type)
	    {
	        if (type == null) return Enumerable.Empty<string>();

            var currentType = type;
	        var typeList = new List<string>();
	        while (currentType != null)
	        {
                typeList.Add(currentType.FullName);
	            currentType = currentType.BaseType;
	        }

	        return typeList;
	    }

		static Type GetEnumerableType(Type type)
		{
			return (
					from intType in type.GetInterfaces().Union(new [] { type })
					where intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof (IEnumerable<>)
					select intType.GetGenericArguments()[0]
					)
				.FirstOrDefault();
		}

		/// <summary>
		/// Creates a cache item
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="data"></param>
		/// <param name="type"></param>
		public virtual void Set<T>(string key, T data, Type type = null) where T : class
		{
			var cacheElement = GetCacheDefaults(data, type);
			if (cacheElement.Enabled)
			{
				var policy = new CacheItemPolicy
					{
						AbsoluteExpiration = DateTime.Now.AddMinutes(cacheElement.CacheLife)
					};
				Cache.Add(new CacheItem(key, data), policy);
			}
		}

		/// <summary>
		/// Looks for an item in cache, if not there executes statement and adds result to cache
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="execute"></param>
		/// <param name="type"></param>
		/// <returns>Expected item</returns>
		public virtual T GetOrExecuteAndAdd<T>(string key, Func<T> execute, Type type = null) where T : class
		{
			if (key == null) { throw new ArgumentNullException("key"); }
			if (execute == null) { throw new ArgumentNullException("execute"); }

			T result = null;

			try
			{
				result = Get<T>(key, type);
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Fail(ex.Message);
			}

			if (null == result)
			{
				result = execute();

				if (result != null && !string.IsNullOrWhiteSpace(key))
				{
					Set(key, result, type);
				}
			}

			return result;
		}
	}
}
