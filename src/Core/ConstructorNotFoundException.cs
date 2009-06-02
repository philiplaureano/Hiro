using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro
{
    /// <summary>
    /// An exception that is thrown whenever the compiler is unable to find a constructor that can be instantiated by the compiled container.
    /// </summary>
    public class ConstructorNotFoundException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the ConstructorNotFoundException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public ConstructorNotFoundException(string message)
            : base(message)
        {
        }
    }
}
