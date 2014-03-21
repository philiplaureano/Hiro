using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hiro
{
    /// <summary>
    /// A helper class that makes easy to build aggregated type predicate expressions.
    /// </summary>
    public static class TypeFilterPredicateExtensions
    {
        /// <summary>
        /// Combines (logically ANDs) two predicate expressions together.
        /// </summary>
        /// <param name="condition">The first condition.</param>
        /// <param name="otherCondition">The second condition.</param>
        /// <returns>A combined predicate that is the result of ANDing both expressions together</returns>
        public static Func<Type, bool> And(this Func<Type, bool> condition, Func<Type, bool> otherCondition)
        {
            return type => condition(type) && otherCondition(type);
        }

        /// <summary>
        /// Combines (logically ORs) two predicate expressions together.
        /// </summary>
        /// <param name="condition">The first condition.</param>
        /// <param name="otherCondition">The second condition.</param>
        /// <returns>A combined predicate that is the result of ORing both expressions together</returns>
        public static Func<Type, bool> Or(this Func<Type, bool> condition, Func<Type, bool> otherCondition)
        {
            return type => condition(type) || otherCondition(type);
        }
        /// <summary>
        /// Negates the given <<paramref name="condition"/>.
        /// </summary>
        /// <param name="condition">The predicate that will be logically negated</param>
        /// <returns>A logically negated predicate.</returns>
        public static Func<Type, bool> Not(this Func<Type, bool> condition)
        {
            return type => !condition(type);
        }
    }
}
