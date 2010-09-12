using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Mono.Cecil;
using Hiro.Loaders;

namespace Hiro.Compilers.Cecil
{
    public class DependencyMapLoader : DependencyMapLoader<MethodDefinition>
    {
        public DependencyMapLoader()
            : this(new ConstructorResolver())
        {
        }
        public DependencyMapLoader(IConstructorResolver<MethodDefinition> constructorResolver)
            : base(constructorResolver)
        {
        }

        public DependencyMapLoader(ITypeLoader typeLoader, IServiceLoader serviceLoader, IDefaultServiceResolver defaultServiceResolver) :
            base(new ConstructorResolver(), typeLoader, serviceLoader, defaultServiceResolver)
        {
        }
        public DependencyMapLoader(IConstructorResolver<MethodDefinition> constructorResolver, ITypeLoader typeLoader, IServiceLoader serviceLoader, IDefaultServiceResolver defaultServiceResolver)
            : base(constructorResolver, typeLoader, serviceLoader, defaultServiceResolver)
        {
        }
        protected override DependencyMap<MethodDefinition> CreateMap(IConstructorResolver<MethodDefinition> constructorResolver)
        {
            return new DependencyMap(constructorResolver) { Injector = new PropertyInjector() };
        }

        protected override IImplementation<MethodDefinition> CreateFactoryCall(string serviceName, Type actualServiceType)
        {
            return new FactoryCall(actualServiceType, serviceName);
        }
    }
}
