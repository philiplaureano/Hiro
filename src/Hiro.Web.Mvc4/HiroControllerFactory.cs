using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

using Hiro.Containers;

namespace Hiro.Web.Mvc4
{
    public class HiroControllerFactory : IControllerFactory
    {
        private readonly IMicroContainer _container;

        public HiroControllerFactory(IMicroContainer container)
        {
            _container = container;
        }

        public IController CreateController(RequestContext requestContext, string controllerName)
        {
            var serviceName = string.Format("{0}Controller", controllerName);
            var controller = _container.GetInstance<IController>(serviceName);
            return controller;
        }

        public SessionStateBehavior GetControllerSessionBehavior(RequestContext requestContext, string controllerName)
        {
            return SessionStateBehavior.Default;
        }

        public void ReleaseController(IController controller)
        {
            var disposable = controller as IDisposable;
            if (disposable != null)
                disposable.Dispose();
        }
    }
}
