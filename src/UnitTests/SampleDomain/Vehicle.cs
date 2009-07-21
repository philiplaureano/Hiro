using System;
using System.Collections.Generic;
using System.Text;

namespace Hiro.UnitTests.SampleDomain
{
    public class Vehicle : IVehicle
    {
        public Vehicle()
        {
        }
        public Vehicle(IPerson driver)
        {
            Driver = driver;
        }

        public IPerson Driver { get; set; }
    }
}
