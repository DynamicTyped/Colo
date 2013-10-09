using System.Configuration;

namespace Colo.Configuration
{
    /// <summary>
    /// Cache section item
    /// </summary>
    public class CacheDefaultValueElement : ConfigurationElement
    {
        private const string _typeProperty = "type";
        /// <summary>
        /// full qualified name of object
        /// </summary>
        [ConfigurationProperty(_typeProperty, IsKey = true)]
        public string Type
        {
            get { return (string)this[_typeProperty]; }
            set { this[_typeProperty] = value; }
        }

        private const string _cacheLife = "cacheLife";
        /// <summary>
        /// How long to cache the object
        /// </summary>
        [ConfigurationProperty(_cacheLife, DefaultValue = 1, IsRequired = true)]
        [IntegerValidator(MinValue = 1)]
        public int CacheLife
        {
            get { return (int)this[_cacheLife]; }
            set { this[_cacheLife] = value; }
        }

        private const string _cacheEnabled = "enabled";
        /// <summary>
        /// If you wish to cache or not
        /// </summary>
        [ConfigurationProperty(_cacheEnabled, DefaultValue = true, IsRequired = false)]
        public bool Enabled 
        {
            get { return (bool) this[_cacheEnabled]; } 
            set {this[_cacheEnabled] = value; } 
        }

    }
}
