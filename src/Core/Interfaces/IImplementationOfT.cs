using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a service implementation that can be emitted in IL.
    /// </summary>
    /// <typeparam name="TMember">The member type.</typeparam>
    public interface IImplementation<TMember> : IStaticImplementation
    {
        /// <summary>
        /// Gets the value indicating the target member.
        /// </summary>
        /// <value>The target member.</value>
        TMember Target { get; }
    }
}
