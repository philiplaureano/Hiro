using Mono.Cecil;
using Mono.Cecil.Cil;

namespace Hiro.Interfaces
{
    public interface IServiceInitializer
    {
        void Initialize(CilWorker worker, ModuleDefinition module, VariableDefinition serviceInstance);
    }
}