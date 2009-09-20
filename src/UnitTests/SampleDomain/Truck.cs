using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.UnitTests.SampleDomain
{
    public class Truck : IVehicle
    {
        public IPerson Driver
        {
            get;
            set;
        }
    }
}
