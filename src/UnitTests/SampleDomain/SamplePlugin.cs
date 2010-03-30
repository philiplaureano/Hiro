using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;

namespace Hiro.UnitTests.SampleDomain
{
    public class SamplePlugin : IContainerPlugin
    {
        public static bool HasBeenCalled
        {
            get; private set;
        }

        public void Initialize(IMicroContainer container)
        {
            HasBeenCalled = true;
        }
    }
}
