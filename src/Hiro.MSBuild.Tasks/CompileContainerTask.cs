using System;
using System.IO;
using Hiro.Containers;
using Hiro.Loaders;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Mono.Cecil;


namespace Hiro.MSBuild.Tasks
{
    /// <summary>
    /// Represents an MSBuild task that builds a Hiro-compiled container from a list of assemblies.
    /// </summary>
    public class CompileContainerTask : Task
    {
        /// <summary>
        /// Gets or sets the value indicating the name of the <see cref="IMicroContainer"/> container implementation..
        /// </summary>
        /// <value>The type name of the container type.</value>
        public string TypeName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating the namespace name of the <see cref="IMicroContainer"/> container implementation.
        /// </summary>
        /// <value>The namespace name of the container type.</value>
        public string NamespaceName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating the assembly name of the <see cref="IMicroContainer"/> container implementation.
        /// </summary>
        /// <value>The assembly name of the container type.</value>
        public string AssemblyName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating the full path and filename of the assemblies that Hiro
        /// will scan to generate the corresponding compiled IOC container.
        /// </summary>
        /// <value>The target assembly filenames.</value>
        [Required]
        public string TargetAssemblies
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the value indicating the full path and filename of the compiled assembly
        /// that will contain the customized <see cref="IMicroContainer"/> type.
        /// </summary>
        [Required]
        public string OutputAssemblyFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Compiles the target IOC container.
        /// </summary>
        /// <returns>Returns <c>true</c> if the operation succeeded. Otherwise, it will return <c>false</c>.</returns>
        public override bool Execute()
        {
            bool result = true;
            try
            {
                string targetPath = GetTargetPath(TargetAssemblies);
                targetPath = string.IsNullOrEmpty(targetPath) ? Environment.CurrentDirectory : targetPath;
                targetPath = Path.GetDirectoryName(targetPath);

                var targetFiles = Path.GetFileName(TargetAssemblies);
                
                // Use the loaded modules from the target assemblies
                // to determine which services will be compiled
                var dependencyMap = new DependencyMap();
                var loader = new ModuleLoader(dependencyMap);
                loader.LoadModulesFrom(targetPath, targetFiles);

                var typeName = TypeName ?? "MicroContainer";
                var namespaceName = NamespaceName ?? "Hiro.Containers";
                var assemblyName = AssemblyName ?? "Hiro.CompiledContainers";

                var compiler = new ContainerCompiler();
                var compiledAssembly = compiler.Compile(typeName, namespaceName, assemblyName, dependencyMap);

                Console.WriteLine("Compiling {0}", OutputAssemblyFileName);
                compiledAssembly.Write(OutputAssemblyFileName);                
            }
            catch (Exception ex)
            {
                result = false;
                Log.LogError(string.Format("Exception thrown: {0}", ex));
            }

            return result;
        }

        private string GetTargetPath(string assemblies)
        {
            var result = string.Empty;
            try
            {
                result = Path.GetFullPath(assemblies);
            }
            catch (IOException)
            {
                result = null;
            }

            return result;
        }
    }
}
