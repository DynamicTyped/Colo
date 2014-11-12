using System;
using System.Configuration;
using System.Linq;

namespace Colo.Configuration
{
    /// <summary>
    /// List of cacheable items
    /// </summary>
    public class CacheDefaultValueElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CacheDefaultValueElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((CacheDefaultValueElement)element).Type;
        }

        public bool ContainsKey(string name)
        {
            return BaseGetAllKeys().Contains(name);
        }

        public new ConfigurationElement this[string name]
        {
            get { return BaseGet(name); }
        }
    }
}
