using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Dependencies;
using Hiro.Containers;
using Microsoft.Practices.ServiceLocation;

namespace Hiro.Web.Mvc4
{
    public class HiroDependencyResolver : IDependencyResolver, IServiceLocator
    {
        private readonly IMicroContainer _container;

        public HiroDependencyResolver(IMicroContainer container)
        {
            _container = container;
        }

        public void Dispose()
        {
        }

        public object GetService(Type serviceType)
        {
            return _container.GetInstance(serviceType, null);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public IEnumerable<TService> GetAllInstances<TService>()
        {
            var items = GetAllInstances((typeof(TService)));
            return items.Cast<TService>();
        }

        public IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetAllInstances(serviceType);
        }

        public TService GetInstance<TService>(string key)
        {
            return _container.GetInstance<TService>(key);
        }

        public TService GetInstance<TService>()
        {
            return _container.GetInstance<TService>();
        }

        public object GetInstance(Type serviceType, string key)
        {
            return _container.GetInstance(serviceType, key);
        }

        public object GetInstance(Type serviceType)
        {
            return _container.GetInstance(serviceType, null);
        }
    }
}
