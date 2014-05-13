using System.Collections.Generic;

namespace WebSite.Core
{
    public class OrderedMap : Dictionary<string, object>
    {
        public T Get<T>(string key)
        {
            return (T)this[key];
        }
    }
}