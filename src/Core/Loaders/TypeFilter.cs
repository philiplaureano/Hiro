using System;
using System.Collections.Generic;
using System.Text;
using Hiro.Interfaces;

namespace Hiro.Loaders
{
    public class TypeFilter : ITypeFilter
    {
        public IList<Type> GetTypes(IEnumerable<Type> items, Predicate<Type> filter)
        {
            var types = new List<Type>();
            foreach (var type in items)
            {
                if (!filter(type))
                    continue;

                types.Add(type);
            }

            return types;
        }
    }
}
