using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Containers
{
    /// <summary>
    /// Represents a type that can extend a <see cref="IMicroContainer"/> type.
    /// </summary>
    public interface IContainerPlugin : IInitialize
    {
    }
}
