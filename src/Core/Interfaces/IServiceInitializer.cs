using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Interfaces
{
    public interface IServiceInitializer
    {
        void Initialize(ILProcessor il, ModuleDefinition module, VariableDefinition serviceInstance);
    }
}