using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;

namespace Hiro.Functors.Core
{
    /// <summary>
    /// Represents a utility class that stores factory functor instances.
    /// </summary>
    public static class FunctorRegistry
    {
        private static readonly Dictionary<string, Func<IMicroContainer, object>> _functors =
            new Dictionary<string, Func<IMicroContainer, object>>();

        private static readonly object _lock = new object();

        /// <summary>
        /// Adds a functor to the registry.
        /// </summary>
        /// <param name="key">The key that uniquely identifies the functor.</param>
        /// <param name="functor">The functor instance.</param>
        public static void AddFunctor(string key, 
            Func<IMicroContainer, object> functor)
        {            
            lock(_lock)
            {
                _functors[key] = functor;
            }
        }

        /// <summary>
        /// Gets the functor associated with the given functor <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The unique functor id.</param>
        /// <returns>A functor that is associated with the given key.</returns>
        public static Func<IMicroContainer, object> GetFunctor(string key)
        {
            Func<IMicroContainer, object> result;

            lock(_lock)
            {
                result = _functors[key];
            }

            return result;
        }

        /// <summary>
        /// Creates the instance with the given <paramref name="container"/> and functor <paramref name="key"/>.
        /// </summary>
        /// <param name="key">The unique functor id.</param>
        /// <param name="container">The host container.</param>
        /// <returns>A new object instance.</returns>
        public static object CreateInstance(string key, IMicroContainer container)
        {
            var functor = GetFunctor(key);
            if (functor == null)
                return null;

            return functor(container);
        }
    }
}
