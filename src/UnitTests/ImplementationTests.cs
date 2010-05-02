using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Hiro.Containers;
using Hiro.Implementations;
using Hiro.Interfaces;
using Hiro.Resolvers;
using Hiro.UnitTests.SampleDomain;
using Moq;
using NUnit.Framework;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class ImplementationTests : BaseFixture
    {
        [Test]
        public void ShouldSelectConstructorWithMostResolvableParameters()
        {
            var targetType = typeof(Vehicle);
            var constructorImplementations = (from c in typeof(Vehicle).GetConstructors()
                                              select new ConstructorCall(c) as IImplementation<ConstructorInfo>).AsEnumerable();

            IImplementation<ConstructorInfo> expectedImplementation = GetExpectedConstructorImplementation(constructorImplementations);

            var map = new Mock<IDependencyContainer>();
            map.Expect(m => m.Contains(It.IsAny<Dependency>())).Returns(true);
            map.Expect(m => m.Dependencies).Returns(new IDependency[] {});

            var constructorResolver = new ConstructorResolver();
            IImplementation<ConstructorInfo> result = constructorResolver.ResolveFrom(targetType, map.Object);

            Assert.AreEqual(expectedImplementation.Target, result.Target);

            map.VerifyAll();
        }

        [Test]
        public void ShouldSelectConstructorWithMostResolvableParametersFromTypeImplementation()
        {
            var map = new Mock<IDependencyContainer>();
            map.Expect(m => m.Contains(It.IsAny<Dependency>())).Returns(true);
            map.Expect(m => m.Dependencies).Returns(new IDependency[] { });

            var expectedConstructor = typeof(Vehicle).GetConstructor(new Type[] { typeof(IPerson) });
            IImplementation<ConstructorInfo> implementation = new TransientType(typeof(Vehicle), map.Object, new ConstructorResolver());

            Assert.AreSame(implementation.Target, expectedConstructor);
        }

        private static IImplementation<ConstructorInfo> GetExpectedConstructorImplementation(IEnumerable<IImplementation<ConstructorInfo>> constructorImplementations)
        {
            var bestParameterCount = 0;
            IImplementation<ConstructorInfo> expectedImplementation = null;
            foreach (var implementation in constructorImplementations)
            {
                var target = implementation.Target;
                var parameterCount = target.GetParameters().Count();

                if (expectedImplementation != null && parameterCount <= bestParameterCount)
                    continue;

                expectedImplementation = implementation;
                bestParameterCount = parameterCount;
            }
            return expectedImplementation;
        }
    }
}
