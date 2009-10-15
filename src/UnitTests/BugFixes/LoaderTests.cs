using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Hiro.Loaders;

namespace Hiro.UnitTests.BugFixes
{
    [TestFixture]
    public class LoaderTests
    {
        [Test]
        public void ShouldLoadAllImplementationsAndInterfaces()
        {
            var assembly = typeof(IDBConnection).Assembly;

            var loader = new DependencyMapLoader();
            var map = loader.LoadFrom(assembly);
            var container = map.CreateContainer();

            var testRepo = container.GetInstance<ITestRepo>();
            Assert.IsNotNull(testRepo);
        }
    }
}
