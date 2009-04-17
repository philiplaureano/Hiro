using LinFu.IoC;
using LinFu.IoC.Interfaces;
using NUnit.Framework;

namespace Hiro.UnitTests
{
    public abstract class BaseFixture
    {
        protected IServiceContainer _container;

        [SetUp]
        public void Init()
        {
            _container = new ServiceContainer();
            _container.LoadFromBaseDirectory("*.dll");
            OnInit();
        }

        [TearDown]
        public void Term()
        {
            OnTerm();
            _container = null;
        }

        protected virtual void OnInit() { }
        protected virtual void OnTerm() { }
    }
}
