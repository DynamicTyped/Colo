﻿using Colo.Configuration;
using ProtoBuf;
using System;
using System.IO;

namespace Colo
{
    /// <summary>
    /// A memory based caching system.  Also uses a memory cache as the back-end management of the keys.
    /// Types must be using protobuf and decorate their class as such
    /// http://code.google.com/p/protobuf-net/
    /// </summary>
    public class ProtobufMemoryCacheProvider : MemoryCacheProvider
    {
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
                    return IsCacheable<T>(type) ? base.Get<T>(key) : null;
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
                if (CachingSection.GetSection.UseCache && IsCacheable<T>(type))
                    base.Set(key, data);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Fail(ex.Message);
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
    }        
}
