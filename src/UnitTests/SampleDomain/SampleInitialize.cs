using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;

namespace Hiro.UnitTests.SampleDomain
{
    public class SampleInitialize : IInitialize
    {
        public void Initialize(IMicroContainer container)
        {
            Container = container;
            NumberOfTimesInitialized++;
        }

        public IMicroContainer Container { get; private set; }
        public int NumberOfTimesInitialized { get; private set; }
    }
}
