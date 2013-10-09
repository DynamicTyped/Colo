using System.Configuration;

namespace Colo.Configuration
{
    public class CachingSection : ConfigurationSection
    {
        private const string SectionName = "Caching";

        public static CachingSection GetSection
        {
            get { return (CachingSection)ConfigurationManager.GetSection(SectionName) ?? new CachingSection(); }
        }

        private const string _path = "path";
        /// <summary>
        /// Location of cache files
        /// </summary>
        [ConfigurationProperty(_path, IsRequired = true)]
        public string Path
        {
            get { return this[_path].ToString(); }
            set { this[_path] = value; }
        }

        private const string _isPathVirtual = "isPathVirtual";
        /// <summary>
        /// Is the location a virtual directory?
        /// </summary>
        [ConfigurationProperty(_isPathVirtual, IsRequired = true)]        
        public bool IsPathVirtual
        {
            get { return (bool)this[_isPathVirtual]; }
            set { this[_isPathVirtual] = value; }
        }

        private const string _cacheLife = "cacheLife";
        /// <summary>
        /// How long to cache objects
        /// </summary>
        [ConfigurationProperty(_cacheLife, DefaultValue=1, IsRequired = true)]     
        [IntegerValidator(MinValue=1)]
        public int CacheLife
        {
            get { return (int)this[_cacheLife]; }
            set { this[_cacheLife] = value; }
        }

        private const string _useCache = "useCache";
        /// <summary>
        /// Turn the cache on or off
        /// </summary>
        [ConfigurationProperty(_useCache, IsRequired = true)]
        public bool UseCache
        {
            get { return (bool)this[_useCache]; }
            set { this[_useCache] = value; } 
        }

        private const string _cacheDefaults = "CacheDefault";
        /// <summary>
        /// Type specific cache configurations
        /// </summary>
        [ConfigurationProperty(_cacheDefaults, IsDefaultCollection = true)]
        public CacheDefaultValueElementCollection CacheDefaults
        {
            get { return (CacheDefaultValueElementCollection)this[_cacheDefaults]; }
        }

    }
}
