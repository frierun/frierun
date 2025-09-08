using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public record ApplicationContext(string Name, string Prefix)
{
    public ApplicationContext(ContractId contractId, string Prefix) : this(contractId.Name, Prefix)
    {
        
    }
}