using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Mvc;
using Hiro.Containers;

namespace Hiro.Web.Mvc4
{
    public static class HiroMvcIntegration
    {
        public static void Install(IMicroContainer container)
        {
            // Replace the default controller factory with Hiro
            var factory = new HiroControllerFactory(container);
            ControllerBuilder.Current.SetControllerFactory(factory);

            // Use Hiro as the dependency resolver
            var resolver = new HiroDependencyResolver(container);
            DependencyResolver.SetResolver(resolver);
            GlobalConfiguration.Configuration.DependencyResolver = resolver;
        }
    }
}
