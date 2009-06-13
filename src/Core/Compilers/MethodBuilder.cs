using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a type that can create methods.
    /// </summary>
    public class MethodBuilder
    {
        /// <summary>
        /// Creates a method on the given host type.
        /// </summary>
        /// <param name="options">The method options object that describes the method to be created.</param>
        /// <returns>A method definition.</returns>
        public MethodDefinition CreateMethod(MethodBuilderOptions options)
        {
            var methodAttributes = MethodAttributes.HideBySig;

            if (options.IsPublic)
            {
                methodAttributes |= MethodAttributes.Public;
            }

            var instanceAttributes = MethodAttributes.NewSlot | MethodAttributes.Virtual;
            methodAttributes |= options.IsStatic ? MethodAttributes.Static : instanceAttributes;

            var targetType = options.HostType;
            var methodName = options.MethodName;
            var returnType = options.ReturnType;
            var parameterTypes = new List<Type>(options.ParameterTypes);

           var newMethod = targetType.DefineMethod(methodName, methodAttributes, returnType, parameterTypes.ToArray(), new Type[0]);

            newMethod.SetReturnType(options.ReturnType);

            return newMethod;
        }
    }
}
