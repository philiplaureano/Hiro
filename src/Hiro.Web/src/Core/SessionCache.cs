using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Hiro.Web
{
    /// <summary>
    /// A class that uses the web session state to cache service instances.
    /// </summary>
    public class SessionCache : ICache
    {
        /// <summary>
        /// Gets or sets the value indicating the key/value pair that will be stored in the current cache instance.
        /// </summary>
        /// <param name="key">The key that will be associated with the cached value.</param>
        /// <returns>The value associated with the key.</returns>
        public object this[string key]
        {
            get
            {
                var context = HttpContext.Current;

                if (context == null)
                    return null;

                return context.Session[key];
            }

            set
            {
                var context = HttpContext.Current;

                if (context == null)
                    return;

                context.Session[key] = value;
            }
        }
    }
}
