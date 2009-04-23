using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinFu.Reflection.Emit;
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
            var methodAttributes = MethodAttributes.HideBySig | MethodAttributes.NewSlot;

            if (options.IsPublic)
            {
                methodAttributes |= MethodAttributes.Public;
                methodAttributes |= MethodAttributes.Virtual;
            }

            if (options.IsStatic)
                methodAttributes |= MethodAttributes.Static;

            var targetType = options.HostType;
            var methodName = options.MethodName;
            var returnType = options.ReturnType;
            var parameterTypes = options.ParameterTypes.ToArray();

            var newMethod = targetType.DefineMethod(methodName, methodAttributes, returnType, parameterTypes);

            newMethod.SetReturnType(options.ReturnType);

            return newMethod;
        }
    }
}
