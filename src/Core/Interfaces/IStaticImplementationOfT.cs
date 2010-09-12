using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a service implementation that can be emitted in IL.
    /// </summary>
    /// <typeparam name="TMember">The member type.</typeparam>
    /// <typeparam name="TMethodBuilder">The method builder type.</typeparam>
    public interface IStaticImplementation<TMember, TMethodBuilder> : IImplementation<TMethodBuilder>
    {
        /// <summary>
        /// Gets the value indicating the target member.
        /// </summary>
        /// <value>The target member.</value>
        TMember Target { get; }

        /// <summary>
        /// Gets the value indicating the type that will be instantiated by this implementation.
        /// </summary>
        /// <value>The target type.</value>
        Type TargetType { get; }
    }
}
