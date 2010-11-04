using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Web
{
    /// <summary>
    /// Represents a cache that can store and retrieve an item based on a string key.
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// Gets or sets the value indicating the key/value pair that will be stored in the current cache instance.
        /// </summary>
        /// <param name="key">The key that will be associated with the cached value.</param>
        /// <returns>The value associated with the key.</returns>
        object this[string key]
        {
            get;
            set;
        }
    }
}
