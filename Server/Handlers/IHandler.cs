using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public interface IHandler
{
    public Application? Application { get; }

    /// <summary>
    /// Discovers already created unmanaged contracts
    /// </summary>
    public IEnumerable<Contract> Discover();
    
    /// <summary>
    /// Returns all possible ways to initialize a contract
    /// </summary>
    public IEnumerable<ContractInitializeResult> Initialize(Contract contract, string prefix);

    /// <summary>
    /// Installs the contract
    /// </summary>
    public Contract Install(Contract contract, ExecutionPlan plan);
    
    /// <summary>
    /// Uninstalls the contract
    /// </summary>
    void Uninstall(Contract contract);
}