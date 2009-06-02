using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a class that describes the options for creating a target method.
    /// </summary>
    public class MethodBuilderOptions
    {
        /// <summary>
        /// Gets or sets the value indicating the method name.
        /// </summary>
        /// <value>The method name.</value>
        public string MethodName { get; set; }

        /// <summary>
        /// Gets or sets the value indicating the method return type.
        /// </summary>
        /// <value>The method return type.</value>
        public Type ReturnType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the method is publicly visible.
        /// </summary>
        /// <value>A boolean value indicating whether or not the method is public.</value>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether or not the method is marked as static.
        /// </summary>
        /// <value>The static method flag.</value>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the type that will hold the newly-created method.
        /// </summary>
        /// <value>The method host.</value>
        public TypeDefinition HostType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating the list of method parameters.
        /// </summary>
        /// <value>The list of parameters for the new method.</value>
        public IEnumerable<Type> ParameterTypes
        {
            get;
            private set;
        }

        /// <summary>
        /// Assigns parameters to the target method.
        /// </summary>
        /// <param name="parameterTypes">The method parameter types.</param>
        public void SetMethodParameters(params Type[] parameterTypes)
        {
            ParameterTypes = parameterTypes;
        }
    }
}
