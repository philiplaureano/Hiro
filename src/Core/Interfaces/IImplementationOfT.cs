using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a service implementation that can be emitted in IL.
    /// </summary>
    public interface IImplementation<TMember> : IImplementation
    {
        /// <summary>
        /// Gets the value indicating the target member.
        /// </summary>
        /// <value>The target member.</value>
        TMember Target { get; }
    }
}
