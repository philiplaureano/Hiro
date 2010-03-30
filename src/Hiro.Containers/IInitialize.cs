using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Containers
{
    /// <summary>
    /// Represents a type that is introduced to the <see cref="IMicroContainer"/> instance upon initialization.
    /// </summary>
    public interface IInitialize
    {
        /// <summary>
        /// Initializes the current type with the given <paramref name="container"/> instance.
        /// </summary>
        /// <param name="container">The <see cref="IMicroContainer"/> instance that instantiated the current t ype.</param>
        void Initialize(IMicroContainer container);
    }
}
