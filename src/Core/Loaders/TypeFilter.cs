using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Loaders
{
    /// <summary>
    /// Represents a type that can filter a list of types.
    /// </summary>
    public class TypeFilter : ITypeFilter
    {
        /// <summary>
        /// Filters a list of given types.
        /// </summary>
        /// <param name="items">The list of types to be filtered.</param>
        /// <param name="filter">The predicate that determines which types should be selected.</param>
        /// <returns>A list of types.</returns>
        public IList<System.Type> GetTypes(IEnumerable<System.Type> items, Predicate<System.Type> filter)
        {
            var types = new List<System.Type>();
            foreach (var type in items)
            {
                if (!filter(type))
                    continue;

                types.Add(type);
            }

            return types;
        }
    }
}
