using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;

namespace SampleAssembly
{
    public class SampleFactory : IFactory<object>
    {
        public object Create()
        {
            return 42;
        }
    }
}
