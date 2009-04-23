using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Hiro.Interfaces;

namespace Hiro.Implementations
{
    public class PropertyImplementation : Implementation<PropertyInfo>
    {
        public PropertyImplementation(PropertyInfo property, IDependencyResolver<PropertyInfo> resolver)
            : base(property, resolver)
        {
        }

        public override void Emit(IServiceEmitterContext context)
        {
            throw new NotImplementedException();
        }
    }
}
