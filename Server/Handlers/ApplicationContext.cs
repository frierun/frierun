using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public record ApplicationContext(string Name, string Prefix)
{
    public ApplicationContext(ContractRef contractRef, string Prefix) : this(contractRef.Name, Prefix)
    {
        
    }
}