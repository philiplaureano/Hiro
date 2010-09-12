using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro
{
    public class HashList<TKey, TItem> : Dictionary<TKey, IList<TItem>>
    {
        public void Add(TKey key, TItem item)
        {
            if (!ContainsKey(key))
                this[key] = new List<TItem>();

            var items = this[key];
            if (items.Contains(item))
                return;
            
            items.Add(item);
        }
    }
}
