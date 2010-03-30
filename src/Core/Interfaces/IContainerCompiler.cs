using Hiro.Containers;
using Hiro.Interfaces;
using Mono.Cecil;

namespace Hiro.Interfaces
{
    /// <summary>
    /// Represents a type that can convert <see cref="IDependencyContainer"/> objects into a compiled assembly that contains an IOC container.
    /// </summary>
    public interface IContainerCompiler
    {
        /// <summary>
        /// Compiles a dependency graph into an IOC container.
        /// </summary>
        /// <param name="dependencyContainer">The <see cref="IDependencyContainer"/> instance that contains the services that will be instantiated by compiled container.</param>
        /// <param name="typeName">The name of the <see cref="IMicroContainer"/> type.</param>
        /// <param name="namespaceName">The namespace name that will be associated with the container type.</param>
        /// <param name="assemblyName">The name of the assembly that will contain the container type.</param>
        /// <returns>An assembly containing the compiled IOC container.</returns>
        AssemblyDefinition Compile(string typeName, string namespaceName, string assemblyName, IDependencyContainer dependencyContainer);
    }
}