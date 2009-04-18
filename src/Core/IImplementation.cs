using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro
{
    public interface IImplementation
    {
        void Accept(IMemberCollector collector);
        IEnumerable<IDependency> GetMissingDependencies(IDependencyContainer map);
    }
}
