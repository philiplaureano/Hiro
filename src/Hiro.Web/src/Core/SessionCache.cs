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
        private readonly IHttpReferenceTracker _tracker;

        /// <summary>
        /// Initializes a new instance of the <see cref="SessionCache"/> class.
        /// </summary>
        /// <param name="tracker">The tracker that will be used to store the created service instances</param>
        public SessionCache(IHttpReferenceTracker tracker)
        {
            if (tracker == null)
                throw new ArgumentNullException("tracker");

            _tracker = tracker;
        }

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

                return _tracker.GetReference(key, context);
            }

            set
            {
                var context = HttpContext.Current;

                if (context == null)
                    return;

                _tracker.SetReference(key, value, context);
            }
        }        
    }
}
