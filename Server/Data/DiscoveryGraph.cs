namespace Frierun.Server.Data;

public class DiscoveryGraph
{
    private readonly HashSet<ContractRef> _toInitialize = [];
    private readonly HashSet<ContractRef> _emptyContracts = [];

    /// <summary>
    /// Prevents infinite recursion during reinitialization
    /// </summary>
    private int _count;

    private const int MaxContracts = 1000;

    public Dictionary<ContractRef, Contract> Contracts { get; } = new();

    public DiscoveryGraph()
    {
    }

    public DiscoveryGraph(DiscoveryGraph graph)
    {
        Contracts = new Dictionary<ContractRef, Contract>(graph.Contracts);
        _toInitialize = [..graph._toInitialize];
        _emptyContracts = [..graph._emptyContracts];
        _count = graph._count;
    }

    /// <summary>
    /// Returns next contract to initialize.
    /// </summary>
    public (ContractRef? Ref, Contract? Contract) Next()
    {
        // reinitializing freshly updated contracts
        while (_toInitialize.Count > 0)
        {
            var contractRef = _toInitialize.First();
            _toInitialize.Remove(contractRef);
            _count++;
            if (_count > MaxContracts)
            {
                throw new Exception("Infinite recursion found during contract reinitialization");
            }

            return (contractRef, Contracts[contractRef]);
        }

        _count = 0;

        // initialize empty contracts
        while (_emptyContracts.Count > 0)
        {
            var contractRef = _emptyContracts.First();
            _emptyContracts.Remove(contractRef);
            if (Contracts.ContainsKey(contractRef))
            {
                continue;
            }

            return (contractRef, null);
        }

        return (null, null);
    }

    /// <summary>
    /// Applies contract initialization result.
    /// </summary>
    /// <returns>True if the result is not conflicting with the graph</returns>
    public bool Apply(ContractRef initializedContractRef, ContractList result)
    {
        try
        {
            foreach (var (contractRef, contract) in result)
            {
                if (Contracts.TryGetValue(contractRef, out var oldContract))
                {
                    Contracts[contractRef] = oldContract.Merge(contract);
                }
                else
                {
                    Contracts[contractRef] = contract;
                }

                if (contractRef != initializedContractRef)
                {
                    _toInitialize.Add(contractRef);
                }
            }
        }
        catch (MergeException)
        {
            return false;
        }

        var initializedContract = result[initializedContractRef];
        foreach (var contractId in initializedContract.DependsOn)
        {
            var contractRef = contractId.Ref;
            if (contractRef == null)
            {
                continue;
            }
            if (!Contracts.ContainsKey(contractRef))
            {
                _emptyContracts.Add(contractRef);
            }
        }

        foreach (var argument in initializedContract.GetArguments())
        {
            foreach (var contractRef in argument.RequiredContracts)
            {
                if (!Contracts.ContainsKey(contractRef))
                {
                    _emptyContracts.Add(contractRef);
                }
            }
        }

        return true;
    }
}