using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Hiro
{
    public class HashedSet<TItem> : IEnumerable<TItem>
    {
        private Dictionary<TItem, object> _entries = new Dictionary<TItem, object>();
        public HashedSet(IEnumerable<TItem> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }

        public void Add(TItem item)
        {
            _entries.Add(item, null);
        }

        public bool Contains(TItem item)
        {
            return _entries.ContainsKey(item);
        }

        public IEnumerator<TItem> GetEnumerator()
        {
            return _entries.Keys.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _entries.Keys.GetEnumerator();
        }
    }
}
