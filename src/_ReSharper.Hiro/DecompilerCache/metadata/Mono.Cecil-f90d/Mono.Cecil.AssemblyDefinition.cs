// Type: Mono.Cecil.AssemblyDefinition
// Assembly: Mono.Cecil, Version=0.9.3.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756
// Assembly location: C:\Users\Phil\Desktop\Development\Hiro\lib\net-2.0\Mono.Cecil.dll

using Mono.Collections.Generic;
using System.IO;

namespace Mono.Cecil
{
    public sealed class AssemblyDefinition : ICustomAttributeProvider, ISecurityDeclarationProvider,
                                             IMetadataTokenProvider
    {
        public AssemblyNameDefinition Name { get; set; }
        public string FullName { get; }
        public Collection<ModuleDefinition> Modules { get; }
        public ModuleDefinition MainModule { get; }
        public MethodDefinition EntryPoint { get; set; }

        #region ICustomAttributeProvider Members

        public MetadataToken MetadataToken { get; set; }
        public bool HasCustomAttributes { get; }
        public Collection<CustomAttribute> CustomAttributes { get; }

        #endregion

        #region ISecurityDeclarationProvider Members

        public bool HasSecurityDeclarations { get; }
        public Collection<SecurityDeclaration> SecurityDeclarations { get; }

        #endregion

        public static AssemblyDefinition CreateAssembly(AssemblyNameDefinition assemblyName, string moduleName,
                                                        ModuleKind kind);

        public static AssemblyDefinition CreateAssembly(AssemblyNameDefinition assemblyName, string moduleName,
                                                        ModuleParameters parameters);

        public static AssemblyDefinition ReadAssembly(string fileName);
        public static AssemblyDefinition ReadAssembly(string fileName, ReaderParameters parameters);
        public static AssemblyDefinition ReadAssembly(Stream stream);
        public static AssemblyDefinition ReadAssembly(Stream stream, ReaderParameters parameters);
        public void Write(string fileName);
        public void Write(Stream stream);
        public void Write(string fileName, WriterParameters parameters);
        public void Write(Stream stream, WriterParameters parameters);
        public override string ToString();
    }
}
