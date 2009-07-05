using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that can filter a list of types.
    /// </summary>
    public interface ITypeFilter
    {
        /// <summary>
        /// Filters a list of given types.
        /// </summary>
        /// <param name="items">The list of types to be filtered.</param>
        /// <param name="filter">The predicate that determines which types should be selected.</param>
        /// <returns>A list of types.</returns>
        IList<Type> GetTypes(IEnumerable<Type> items, Predicate<Type> filter);
    }
}
