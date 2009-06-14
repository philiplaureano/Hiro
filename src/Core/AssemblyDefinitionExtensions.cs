using System.IO;
using System.Reflection;
using Mono.Cecil;

namespace Hiro
{
    public static class AssemblyDefinitionExtensions
    {
        /// <summary>
        /// Converts an <see cref="AssemblyDefinition"/>
        /// into a running <see cref="Assembly"/>.
        /// </summary>
        /// <param name="definition">The <see cref="AssemblyDefinition"/> to convert.</param>
        /// <returns>An <see cref="Assembly"/> that represents the <see cref="AssemblyDefinition"/> instance.
        /// </returns>
        public static Assembly ToAssembly(this AssemblyDefinition definition)
        {
            Assembly result = null;
            using (var stream = new MemoryStream())
            {
                // Persist the assembly to the stream
                AssemblyFactory.SaveAssembly(definition, stream);
                var buffer = stream.GetBuffer();
                result = Assembly.Load(buffer);
            }

            return result;
        }
    }
}
