using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Hiro.Interfaces;

namespace Hiro.Implementations
{
    public class ConstructorImplementation : Implementation<ConstructorInfo>
    {
        public ConstructorImplementation(ConstructorInfo constructor, IDependencyResolver<ConstructorInfo> resolver)
            : base(constructor, resolver)
        {
        }

        public override void Emit(IServiceEmitterContext context)
        {
            throw new NotImplementedException();
        }
    }
}
