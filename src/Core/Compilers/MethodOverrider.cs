using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using LinFu.Reflection.Emit;
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
        public void AddOverrideFor(MethodInfo targetMethod, TypeDefinition hostType)
        {
            var module = hostType.Module;
            var options = new MethodBuilderOptions();

            options.IsPublic = true;
            options.MethodName = targetMethod.Name;

            var parameterTypes = (from param in targetMethod.GetParameters()
                                 select param.ParameterType).ToArray();

            options.HostType = hostType;
            options.SetMethodParameters(parameterTypes);
            options.ReturnType = targetMethod.ReturnType;

            var builder = new MethodBuilder();
            builder.CreateMethod(options);            
        }
    }
}
