using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Hiro.Implementations;
using Hiro.Interfaces;
using Mono.Cecil;

namespace Hiro.Resolvers
{
    public class ConstructorResolver : ConstructorResolver<MethodDefinition>
    {
        /// <summary>
        /// Creates the <see cref="IImplementation{TMethodBuilder}"/> instance that will generate the given constructor call.
        /// </summary>
        /// <param name="constructor">The target constructor.</param>
        /// <returns>The target implementation.</returns>
        protected override IStaticImplementation<ConstructorInfo, MethodDefinition> CreateConstructorCall(ConstructorInfo constructor)
        {
            return new ConstructorCall(constructor);
        }
    }
}
