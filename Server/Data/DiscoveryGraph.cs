using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class DiscoveryGraph
{
    private readonly HashSet<ContractId> _emptyContracts = new();
    private readonly Queue<Contract> _uninitializedContracts = new();
    public Dictionary<ContractId, Contract> Contracts { get; } = new();

    public DiscoveryGraph()
    {
    }
    
    public DiscoveryGraph(DiscoveryGraph graph)
    {
        Contracts = new Dictionary<ContractId, Contract>(graph.Contracts);
        _emptyContracts = [..graph._emptyContracts];
        _uninitializedContracts = new Queue<Contract>(graph._uninitializedContracts);
    }

    /// <summary>
    /// Returns next contract to initialize.
    /// </summary>
    public (ContractId?, Contract?) Next()
    {
        if (_uninitializedContracts.Count == 0 && _emptyContracts.Count == 0)
        {
            return (null, null);
        }

        while (_uninitializedContracts.Count > 0)
        {
            var contract = _uninitializedContracts.Dequeue();
            if (Contracts.ContainsKey(contract.Id))
            {
                continue;
            }

            return (contract.Id, contract);
        }

        while (_emptyContracts.Count > 0)
        {
            var contractId = _emptyContracts.First();
            _emptyContracts.Remove(contractId);
            if (Contracts.ContainsKey(contractId))
            {
                continue;
            }

            return (contractId, null);
        }

        return (null, null);
    }
    
    /// <summary>
    /// Applies contract initialization result.
    /// </summary>
    public void Apply(InstallerInitializeResult result)
    {
        Contracts[result.Contract.Id] = result.Contract;
        
        foreach (var additionalContract in result.AdditionalContracts)
        {
            _uninitializedContracts.Enqueue(additionalContract);
        }

        foreach (var contractId in result.RequiredContracts)
        {
            if (!Contracts.ContainsKey(contractId))
            {
                _emptyContracts.Add(contractId);
            }
        }        
    }
}