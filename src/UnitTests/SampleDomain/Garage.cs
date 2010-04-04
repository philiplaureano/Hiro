using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro.UnitTests.SampleDomain
{
    public class Garage
    {
        public Garage(IVehicle vehicle)
        {
            Vehicle = vehicle;
        }

        public IVehicle Vehicle { get; set; }        
    }
}
