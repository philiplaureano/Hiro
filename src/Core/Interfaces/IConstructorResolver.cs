using System;
using System.Reflection;
using Hiro.Interfaces;

namespace Hiro.Resolvers
{
    /// <summary>
    /// Represents a type that determines the constructor that will be used to instantiate a particular service implementation.
    /// </summary>
    public interface IConstructorResolver
    {
        /// <summary>
        /// Determines which constructor implementation should be used from a given <see cref="IDependencyContainer"/> instance.
        /// </summary>
        /// <param name="targetType">The target type that contains list of constructors to be resolved.</param>
        /// <param name="container">The dependency container that holds the current set of dependencies.</param>
        /// <returns>An implementation that can instantiate the object associated with the constructor.</returns>
        IImplementation<ConstructorInfo> ResolveFrom(Type targetType, IDependencyContainer container);
    }
}