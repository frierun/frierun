using Frierun.Server.Data;

namespace Frierun.Tests.Factories;

public record ContractEntry<TContract>(ContractRef<TContract> Ref, TContract Contract)
    where TContract : Contract
{
    public ContractEntry(string name, TContract contract) : this(new ContractRef<TContract>(name), contract)
    {
        
    }
    
    public ContractId<TContract> Id => new(Guid.Empty, Ref.Name);
    
    public ContractEntry<TContract> With(Func<TContract, TContract> customizer) => new(Ref, customizer(Contract));

    public static implicit operator KeyValuePair<ContractRef, Contract>(ContractEntry<TContract> entry) =>
        new(entry.Ref, entry.Contract);
}