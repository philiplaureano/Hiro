using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Mono.Cecil;

namespace Hiro.Compilers
{
    /// <summary>
    /// Represents a type that adds method overrides for interface methods.
    /// </summary>
    public class MethodOverrider
    {
        /// <summary>
        /// Adds a method override for a particular <paramref name="targetMethod"/>.
        /// </summary>
        /// <param name="targetMethod">The target method.</param>
        /// <param name="hostType">The type that will host the new method.</param>
        /// <returns>The overridden method.</returns>
        public MethodDefinition AddOverrideFor(MethodInfo targetMethod, TypeDefinition hostType)
        {
            var module = hostType.Module;
            var options = new MethodBuilderOptions();

            options.IsPublic = true;
            options.MethodName = targetMethod.Name;

            var parameterTypes = new List<Type>();
            foreach (var param in targetMethod.GetParameters())
            {
                parameterTypes.Add(param.ParameterType);
            }

            options.HostType = hostType;
            options.SetMethodParameters(parameterTypes.ToArray());
            options.ReturnType = targetMethod.ReturnType;

            var builder = new MethodBuilder();
            return builder.CreateMethod(options);
        }
    }
}
