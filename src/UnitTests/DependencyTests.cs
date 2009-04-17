using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Hiro.UnitTests.SampleDomain;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class DependencyTests : BaseFixture
    {
        public void ShouldBeEqualIfServiceNameAndServiceTypeAreTheSame()
        {
            var first = new Dependency(string.Empty, typeof(IPerson));
            var second = new Dependency(string.Empty, typeof(IPerson));

            Assert.AreEqual(first, second);
        }
    }
}
