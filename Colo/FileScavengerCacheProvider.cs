using System;
using System.Runtime.Caching;
using System.Threading;
using System.IO;
using System.Web;
using Colo.Configuration;

namespace Colo
{
	/// <summary>
	/// This is a watcher process only.  Uses a memory based cache object to store the location of files and will delete them when the key is removed from the cache
	/// The object is not saved to the cache, only string.empty.  Uses locking on the cache dictionary and file pieces
	/// </summary>
	public class FileScavengerCacheProvider : BaseCacheProvider
	{
		protected override ObjectCache Cache
		{
			get { return MemoryCache.Default; }
		}

		public static string Path
		{
            get { return CachingSection.GetSection.IsPathVirtual ? HttpContext.Current.Server.MapPath(CachingSection.GetSection.Path) : CachingSection.GetSection.Path; }
		}

		/// <summary>
		/// Takes a file name and appends the path from the configuration file.  Takes the IsPathVirtual value into account
		/// </summary>
		/// <param name="fileName">file to add path to</param>
		/// <returns>combined path and filename</returns>
		public static string CombinePathAndFileName(string fileName)
		{
			return System.IO.Path.Combine(Path, fileName);
		}

		/// <summary>
		/// Does the core getting of data
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <returns>Default implementation returns null as nothing is in the cache for this object</returns>        
		protected virtual T InternalGet<T>(string key) where T : class
		{
			return null;
		}

		/// <summary>
		/// Core logic for saving the object.  This does nothing for the pure scavenger process
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key"></param>
		/// <param name="data"></param>
		protected virtual void InternalSet<T>(string key, T data) where T : class
		{
		}

		/// <summary>
		/// Gets an item from cache.  Uses locking to prevent reading from items not finished writing or being deleted
		/// </summary>
		/// <typeparam name="T">type of the data being saved</typeparam>
		/// <param name="key">full file path</param>
		/// <param name="type">If T is a some form of enumeration, then type must be passed in.  It is the type of the enumeration. For example if T = List of int, type must be typeof(int).  If T is double or some other class type can be null  If you are a scavenger only process this is not used</param>
		/// <returns></returns>
		public override T Get<T>(string key, Type type = null)
		{
			try
			{
			    if (CachingSection.GetSection.UseCache)
				{
					var locker = Cache.Get(key) as Lazy<ReaderWriterLockSlim>;

				    if (locker != null)
				    {
				        try
				        {
				            locker.Value.EnterReadLock();
				            return InternalGet<T>(key);
				        }
				        finally
				        {
				            locker.Value.ExitReadLock();
				        }
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

		public void Set(string key)
		{
			Set(key, string.Empty);
		}

		/// <summary>
		/// Saves the pointer to the file to cache
		/// </summary>
		/// <typeparam name="T">Type of object</typeparam>
		/// <param name="key">full file name</param>
		/// <param name="data">data to save</param>
		/// <param name="type">If T is a some form of enumeration, then type must be passed in.  It is the type of the enumeration. For example if T = List of int, type must be typeof(int).  If T is double or some other class type can be null.  Ignored for filescavenger process</param>
		public override void Set<T>(string key, T data, Type type = null)
		{
			try
			{
			    var cacheElement = GetCacheDefaults(data, type);
				if (CachingSection.GetSection.UseCache && cacheElement.Enabled)
				{
					// lock the locker dictionary, add this lock, free the dictionary, lock the file, do work, unlock file
					var locker = new Lazy<ReaderWriterLockSlim>();

					locker = Cache.AddOrGetExisting(
						key,
						locker,
						new CacheItemPolicy()
						{
                            AbsoluteExpiration = DateTime.Now.AddMinutes(cacheElement.CacheLife),
							RemovedCallback = RemoveItem
						}) as Lazy<ReaderWriterLockSlim>
						?? locker;

					try
					{
						locker.Value.EnterWriteLock();
						InternalSet(key, data);
					}
					finally
					{
						locker.Value.ExitWriteLock();
					}
				}

			}
			catch (Exception ex)
			{
                System.Diagnostics.Debug.Fail(ex.Message);
			}
		}

		/// <summary>
		/// cache policy uses a scavenger process to clean up when a cache has expired.  This is a callback method when an item expires.  It cleans up the file cache
		/// and the item in the lock dictionary.
		/// </summary>
		/// <param name="args"></param>
		static void RemoveItem(CacheEntryRemovedArguments args)
		{
			if (null != args && null != args.CacheItem)
			{
				var key = args.CacheItem.Key;
				var locker = args.CacheItem.Value as Lazy<ReaderWriterLockSlim>;

				if (File.Exists(key) && locker != null)
				{
					try
					{
						locker.Value.EnterWriteLock();
						File.Delete(key);
					}
					catch (Exception ex) 
                    {
                        System.Diagnostics.Debug.Fail(ex.Message);
                    }
					finally
					{
						locker.Value.ExitWriteLock();
						locker.Value.Dispose();
					}
				}
			}
		}
	}
}
