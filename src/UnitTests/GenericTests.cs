using System.Collections.Generic;
using NUnit.Framework;

namespace Hiro.UnitTests
{
    [TestFixture]
    public class GenericTests
    {
        [Test]
        public void ShouldBeAbleToInstantiateGenericTypes()
        {
            var map = new DependencyMap();
            map.AddService(typeof(IList<>), typeof(List<>));

            var container = map.CreateContainer();
            var list = container.GetInstance<IList<int>>();
            Assert.IsNotNull(list);
        }

        [Test]
        public void ShouldBeAbleToInstantiateGenericSingletonTypes()
        {
            var map = new DependencyMap();
            map.AddSingletonService(typeof(IList<>), typeof(List<>));

            var container = map.CreateContainer();
            var list = container.GetInstance<IList<int>>();
            Assert.IsNotNull(list);

            var otherList = container.GetInstance<IList<int>>();
            Assert.AreSame(list, otherList);
        }

        [Test]
        [Ignore("TODO: Finish this feature")]
        public void ShouldBeAbleToInstantiateNamedGenericType()
        {
            var map = new DependencyMap();
            map.AddService("List", typeof(IEnumerable<>), typeof(List<>));
            map.AddService("Queue", typeof(IEnumerable<>), typeof(Queue<>));

            var container = map.CreateContainer();
            var list = container.GetInstance<IEnumerable<int>>("List");
            Assert.IsNotNull(list);

            var queue = container.GetInstance<IEnumerable<int>>("Queue");
            Assert.IsNotNull(queue);

            Assert.IsFalse(queue.GetType() != list.GetType());
        }
    }
}