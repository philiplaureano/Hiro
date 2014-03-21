using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Hiro.Loaders;

namespace Hiro.Web.Mvc4
{
    public static class DependencyMapMvcExtensions 
    {
        /// <summary>
        /// Registers all controller types from the given assembly.
        /// </summary>
        /// <param name="map">The target dependency map.</param>
        /// <typeparam name="T">A type that is hosted within the target assembly.</typeparam>
        public static void RegisterAllControllersFromAssemblyOf<T>(this DependencyMap map)
        {
            map.RegisterAllControllersFrom(typeof(T).Assembly);
        }

        /// <summary>
        /// Registers all controller types from the given assembly.
        /// </summary>
        /// <param name="map">The target dependency map.</param>
        /// <param name="assembly">The assembly that contains the controller types being registered.</param>
        public static void RegisterAllControllersFrom(this DependencyMap map, Assembly assembly)
        {
            map.RegisterAllControllersFrom(new[] { assembly });
        }

        /// <summary>
        /// Registers all controller types from the given assemblies.
        /// </summary>
        /// <param name="map">The target dependency map.</param>
        /// <param name="assemblies">The list of assemblies that contain the controller types being registered.</param>
        public static void RegisterAllControllersFrom(this DependencyMap map, IEnumerable<Assembly> assemblies)
        {
            Action<DependencyMap, Type> registerTypeAction = (dependencyMap, controllerType) =>
            {
                var typeName = controllerType.Name;
                dependencyMap.AddService(typeName, typeof(IController), controllerType);
            };

            map.AddServicesFrom(assemblies, TypeFilters.IsDerivedFrom<IController>(), registerTypeAction);
        }        
    }
}
