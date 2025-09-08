using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public record ContractEntry<TContract>(ContractId<TContract> Id, TContract Contract)
    where TContract : Contract
{
    public ContractEntry(string name, TContract contract) : this(new ContractId<TContract>(name), contract)
    {
        
    }
    
    public ContractEntry<TContract> With(Func<TContract, TContract> customizer) => new(Id, customizer(Contract));

    public static implicit operator KeyValuePair<ContractId, Contract>(ContractEntry<TContract> entry) =>
        new(entry.Id, entry.Contract);
}