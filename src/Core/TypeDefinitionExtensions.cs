using System;
using System.Collections.Generic;
using System.Text;
using Mono.Cecil.Cil;
using Mono.Cecil;
using System.Reflection;

namespace Hiro
{
    /// <summary>
    /// A class that extends the <see cref="TypeDefinition"/>
    /// class with features similar to the features in the
    /// System.Reflection.Emit namespace.
    /// </summary>
    public static class TypeDefinitionExtensions
    {
        /// <summary>
        /// Adds a default constructor to the target type.
        /// </summary>
        /// <param name="targetType">The type that will contain the default constructor.</param>
        /// <returns>The default constructor.</returns>
        public static MethodDefinition AddDefaultConstructor(this TypeDefinition targetType)
        {
            var parentType = typeof(object);

            return AddDefaultConstructor(targetType, parentType);
        }

        /// <summary>
        /// Adds a default constructor to the target type.
        /// </summary>
        /// <param name="parentType">The base class that contains the default constructor that will be used for constructor chaining..</param>
        /// <param name="targetType">The type that will contain the default constructor.</param>
        /// <returns>The default constructor.</returns>
        public static MethodDefinition AddDefaultConstructor(this TypeDefinition targetType, Type parentType)
        {
            var module = targetType.Module;
            var voidType = module.Import(typeof(void));
            var methodAttributes = Mono.Cecil.MethodAttributes.Public | Mono.Cecil.MethodAttributes.HideBySig
                                   | Mono.Cecil.MethodAttributes.SpecialName | Mono.Cecil.MethodAttributes.RTSpecialName;


            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var objectConstructor = parentType.GetConstructor(flags, null, new Type[0], null);

            // Revert to the System.Object constructor
            // if the parent type does not have a default constructor
            if (objectConstructor == null)
                objectConstructor = typeof(object).GetConstructor(new Type[0]);

            var baseConstructor = module.Import(objectConstructor);

            // Define the default constructor
            var ctor = new MethodDefinition(".ctor", methodAttributes, voidType)
            {
                CallingConvention = MethodCallingConvention.StdCall,
                ImplAttributes = (Mono.Cecil.MethodImplAttributes.IL | Mono.Cecil.MethodImplAttributes.Managed)
            };

            var IL = ctor.Body.CilWorker;

            // Call the constructor for System.Object, and exit
            IL.Emit(OpCodes.Ldarg_0);
            IL.Emit(OpCodes.Call, baseConstructor);
            IL.Emit(OpCodes.Ret);

            targetType.Constructors.Add(ctor);

            return ctor;
        }

        /// <summary>
        /// Adds a new method to the <paramref name="typeDef">target type</paramref>.
        /// </summary>
        /// <param name="typeDef">The type that will hold the newly-created method.</param>
        /// <param name="attributes">The <see cref="MethodAttributes"/> parameter that describes the characteristics of the method.</param>
        /// <param name="methodName">The name to be given to the new method.</param>
        /// <param name="returnType">The method return type.</param>        
        /// <param name="parameterTypes">The list of argument types that will be used to define the method signature.</param>
        /// <param name="genericParameterTypes">The list of generic argument types that will be used to define the method signature.</param>
        /// <returns>A <see cref="MethodDefinition"/> instance that represents the newly-created method.</returns>
        public static MethodDefinition DefineMethod(this TypeDefinition typeDef, string methodName,
            Mono.Cecil.MethodAttributes attributes, Type returnType, Type[] parameterTypes, Type[] genericParameterTypes)
        {
            var method = new MethodDefinition(methodName, attributes, null);

            typeDef.Methods.Add(method);

            //Match the generic parameter types
            foreach (var genericParameterType in genericParameterTypes)
            {
                method.AddGenericParameter(genericParameterType);
            }

            // Match the parameter types
            method.AddParameters(parameterTypes);

            // Match the return type
            method.SetReturnType(returnType);

            return method;
        }     
    }
}
