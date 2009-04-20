using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that can infer the service dependencies from a type member.
    /// </summary>
    /// <typeparam name="TMember">The member type.</typeparam>
    public interface IDependencyResolver<TMember>
    {
        /// <summary>
        /// Determines the dependencies from a type member.
        /// </summary>
        /// <param name="member">The type member.</param>
        /// <returns>The list of dependencies required to invoke the target member.</returns>
        IEnumerable<IDependency> GetDependenciesFrom(TMember member);
    }
}
