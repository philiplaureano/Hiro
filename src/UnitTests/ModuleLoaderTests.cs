using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hiro.Interfaces;
using Hiro.Loaders;
using NUnit.Framework;
using SampleAssembly;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class ModuleLoaderTests
    {
        [Test]
        public void ShouldLoadSampleModule()
        {
            var map = new DependencyMap();
            var sampleModule = new SampleModule();
            var loader = new ModuleLoader(map);
            loader.LoadModule(sampleModule);

            Assert.IsTrue(sampleModule.Invoked);
            Assert.IsTrue(map.Contains(typeof(IList<int>)));
        }

        [Test]
        public void ShouldLoadSampleModuleFromGivenDirectory()
        {
            var map = new DependencyMap();
            var loader = new ModuleLoader(map);

            var targetDirectory = Path.GetDirectoryName(typeof (SampleModule).Assembly.Location);
            loader.LoadModulesFrom(targetDirectory, "*.dll");
            Assert.IsTrue(map.Contains(typeof(IList<int>)));
        }
    }
}
