using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro.Web.UnitTests.Caching
{
    public class MockCache : ICache
    {
        private readonly Dictionary<string, object> _entries = new Dictionary<string, object>();

        public MockCache()
        {
            return;
        }

        public object this[string key]
        {
            get
            {
                if (!_entries.ContainsKey(key))
                    return null;

                return _entries[key];
            }
            set
            {
                _entries[key] = value;
            }
        }
    }
}
