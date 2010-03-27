using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;

namespace Hiro.UnitTests.SampleDomain
{
    public class SampleContainerAwareType
    {
        private readonly IMicroContainer _container;

        public SampleContainerAwareType(IMicroContainer container)
        {
            _container = container;
        }

        public IMicroContainer Container
        {
            get
            {
                return _container;
            }
        }
    }
}
