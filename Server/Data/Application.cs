namespace Frierun.Server.Data;


public class Application 
{
    public required string Name { get; init; }
    public Package? Package { get; init; }
    public string? Url { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<Contract> Contracts { get; init; } = [];
    public IReadOnlyList<string> RequiredApplications { get; init; } = [];
    
    /// <summary>
    /// Get contract by id.
    /// </summary>
    public Contract GetContract(ContractId contractId)
    {
        return Contracts.Single(contract => contract.Id == contractId);
    }

    /// <summary>
    /// Get contract by id.
    /// </summary>
    public T GetContract<T>(ContractId<T> contractId)
        where T : Contract
    {
        return (T)GetContract((ContractId)contractId);
    }
}