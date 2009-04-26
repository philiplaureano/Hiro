using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Implementations
{
    /// <summary>
    /// Represents an implementation that emits a constructor call.
    /// </summary>
    public class ConstructorImplementation : Implementation<ConstructorInfo>
    {
        /// <summary>
        /// Initializes a new instance of the ConstructorImplementation class.
        /// </summary>
        /// <param name="constructor">The target constructor.</param>
        /// <param name="resolver">The dependency resolver.</param>
        public ConstructorImplementation(ConstructorInfo constructor, IDependencyResolver<ConstructorInfo> resolver)
            : base(constructor, resolver)
        {
        }
    }
}
