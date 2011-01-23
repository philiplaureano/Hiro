using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SampleAssembly
{
    public class SampleClassWithEnumerableBazDependency : IFizz
    {
        private readonly IEnumerable<IBaz<int>> _services;

        public SampleClassWithEnumerableBazDependency()
        {            
        }
        public SampleClassWithEnumerableBazDependency(IEnumerable<IBaz<int>> services)
        {
            _services = services;
        }

        public IEnumerable<IBaz<int>> Services
        {
            get { return _services; }
        }
    }
}
