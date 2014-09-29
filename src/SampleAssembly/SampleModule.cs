using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro;
using Hiro.Interfaces;

namespace SampleAssembly
{
    public class SampleModule : IModule
    {
        private bool _invoked;

        public void Load(DependencyMap map)
        {
            _invoked = true;

            // Add a sample implementation
            map.AddService<IList<int>, List<int>>();
        }

        public bool Invoked
        {
            get { return _invoked; }
        }
    }
}
