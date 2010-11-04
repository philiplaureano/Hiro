using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hiro.Containers;
using Hiro.Resolvers;
using NUnit.Framework;
using Hiro.Web.UnitTests.Caching;
using Hiro.Implementations;
using Moq;
namespace Hiro.Web.UnitTests
{
    [TestFixture]
    public class SessionCachingTests
    {
        [Test]
        public void ShouldCallCacheOnCachedServiceType()
        {
            var map = new DependencyMap();
            var dependency = new Dependency(typeof(IFoo));
            var implementation = new TransientType(typeof(Foo), map, new ConstructorResolver());
            var cachedImplementation = new CachedInstantiation(implementation);

            map.AddSingletonService<ICache, MockCache>();
            map.AddService(dependency, cachedImplementation);

            // Compile the container
            var container = map.CreateContainer();
            
            // Grab the service instance
            var firstResult = container.GetInstance<IFoo>();
            var secondResult = container.GetInstance<IFoo>();

            Assert.IsNotNull(firstResult);
            Assert.AreSame(firstResult, secondResult);
        }
    }
}
