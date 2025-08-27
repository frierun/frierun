using System.Diagnostics;
using Frierun.Server.Handlers;

namespace Frierun.Server.Data;

public class DiscoveryGraph
{
    
    private readonly HashSet<ContractId> _toInitialize = [];
    private readonly HashSet<ContractId> _emptyContracts = [];

    /// <summary>
    /// Prevents infinite recursion during reinitialization
    /// </summary>
    private int _count;
    private const int MaxContracts = 1000;

    public Dictionary<ContractId, Contract> Contracts { get; } = new();

    public DiscoveryGraph()
    {
    }

    public DiscoveryGraph(DiscoveryGraph graph)
    {
        Contracts = new Dictionary<ContractId, Contract>(graph.Contracts);
        _toInitialize = [..graph._toInitialize];
        _emptyContracts = [..graph._emptyContracts];
        _count = graph._count;
    }

    /// <summary>
    /// Returns next contract to initialize.
    /// </summary>
    public (ContractId?, Contract?) Next()
    {
        // reinitializing freshly updated contracts
        while (_toInitialize.Count > 0)
        {
            var contractId = _toInitialize.First();
            _toInitialize.Remove(contractId);
            _count++;
            if (_count > MaxContracts)
            {
                throw new Exception("Infinite recursion found during contract reinitialization");
            }

            return (contractId, Contracts[contractId]);
        }

        _count = 0;
        
        // initialize empty contracts
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
    /// <returns>True if the result is not conflicting with the graph</returns>
    public bool Apply(ContractInitializeResult result)
    {
        Debug.Assert(result.Contract.Handler != null, "Initialized contract must have a handler");

        try
        {
            var contract = result.Contract;
            if (Contracts.TryGetValue(contract, out var initializedContract))
            {
                contract = contract.Merge(initializedContract);
            }

            Contracts[contract] = contract;
        }
        catch (MergeException)
        {
            return false;
        }

        try
        {
            foreach (var additionalContract in result.AdditionalContracts)
            {
                if (Contracts.TryGetValue(additionalContract, out var initializedContract))
                {
                    var contract = initializedContract.Merge(additionalContract);
                    Contracts[contract] = contract;
                }
                else
                {
                    Contracts[additionalContract] = additionalContract;
                }
                _toInitialize.Add(additionalContract);
            }
        }
        catch (MergeException)
        {
            return false;
        }

        foreach (var contractId in result.Contract.DependsOn)
        {
            if (!Contracts.ContainsKey(contractId))
            {
                _emptyContracts.Add(contractId);
            }
        }
        
        foreach (var argument in result.Contract.GetArguments())
        {
            foreach (var contractId in argument.RequiredContracts)
            {
                if (!Contracts.ContainsKey(contractId))
                {
                    _emptyContracts.Add(contractId);
                }
            }
        }

        return true;
    }
}