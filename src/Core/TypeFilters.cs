using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiro
{
    /// <summary>
    /// Represents a helper class that generates type filter predicates.
    /// </summary>
    public static class TypeFilters
    {
        /// <summary>
        /// Generates a type filter that limits the registered types to the ones that are derived from type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The base type that all the selected types must derive from.</typeparam>
        /// <returns>A predicate with the given type filter.</returns>
        public static Func<Type, bool> IsDerivedFrom<T>()
        {
            return type => type.IsClass && !type.IsInterface && type.IsPublic && typeof(T).IsAssignableFrom(type);
        }

        /// <summary>
        /// Generates a type filter that limits the registered types to the ones that exactly match the given <see cref="nameSpace"/>.
        /// </summary>
        /// <param name="nameSpace">The namespace that contains the target types.</param>
        /// <returns>A predicate with the given type filter.</returns>
        public static Func<Type, bool> NamespaceIs(string nameSpace)
        {
            return type => type.Namespace == nameSpace;
        }

        /// <summary>
        /// Generates a type filter that limits the registered types to the ones that fall under the given <see cref="nameSpace"/>.
        /// </summary>
        /// <param name="nameSpace">The namespace that contains the target types.</param>
        /// <returns>A predicate with the given type filter.</returns>
        public static Func<Type, bool> NamespaceIsUnder(string nameSpace)
        {
            return type => (type.Namespace ?? string.Empty).StartsWith(nameSpace);
        }
   }
}
