using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.Containers
{
    /// <summary>
    /// Represents a factory type.
    /// </summary>
    /// <typeparam name="T">The type of object that will be created by the factory itself.</typeparam>
    public interface IFactory<T>
    {
        /// <summary>
        /// Creates the given <typeparamref name="T"/> type.
        /// </summary>
        /// <returns>The <typeparamref name="T"/> instance.</returns>
        T Create();
    }
}
