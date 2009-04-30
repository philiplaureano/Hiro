using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hiro.UnitTests.SampleDomain
{
    public class Truck : IVehicle 
    {
        public IPerson Driver
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
