using System.Diagnostics;
using System.Text.RegularExpressions;
using Frierun.Server.Handlers;

namespace Frierun.Server.Data;

public class DiscoveryGraph
{
    private readonly HashSet<ContractId> _toReinitialize = [];
    private readonly HashSet<ContractId> _emptyContracts = [];
    private readonly Dictionary<ContractId, Contract> _uninitializedContracts = new();
    
    /// <summary>
    /// Prevents infinite recursion during reinitialization
    /// </summary>
    private readonly HashSet<ContractId> _reinitializeRecursion = [];

    
    public Dictionary<ContractId, Contract> Contracts { get; } = new();

    public DiscoveryGraph()
    {
    }

    public DiscoveryGraph(DiscoveryGraph graph)
    {
        Contracts = new Dictionary<ContractId, Contract>(graph.Contracts);
        _toReinitialize = [..graph._toReinitialize];
        _emptyContracts = [..graph._emptyContracts];
        _uninitializedContracts = new Dictionary<ContractId, Contract>(graph._uninitializedContracts);
        _reinitializeRecursion = [..graph._reinitializeRecursion];
    }

    /// <summary>
    /// Returns next contract to initialize.
    /// </summary>
    public (ContractId?, Contract?) Next()
    {
        // reinitializing freshly updated contracts
        while (_toReinitialize.Count > 0)
        {
            var contractId = _toReinitialize.First();
            _toReinitialize.Remove(contractId);
            if (!_reinitializeRecursion.Add(contractId))
            {
                throw new Exception("Infinite recursion found during contract reinitialization");
            }
            return (contractId, Contracts[contractId]);
        }

        _reinitializeRecursion.Clear();

        // initialize contracts which were defined but not initialized yet
        while (_uninitializedContracts.Count > 0)
        {
            var (contractId, contract) = _uninitializedContracts.First();
            _uninitializedContracts.Remove(contractId);
            Debug.Assert(!Contracts.ContainsKey(contractId));

            return (contractId, contract);
        }

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
        
        Contracts[result.Contract] = result.Contract;

        try
        {
            foreach (var additionalContract in result.AdditionalContracts)
            {
                if (Contracts.TryGetValue(additionalContract, out var initializedContract))
                {
                    var contract = initializedContract.Merge(additionalContract);
                    Contracts[contract] = contract;
                    _toReinitialize.Add(contract);
                    continue;
                }

                if (_uninitializedContracts.TryGetValue(additionalContract, out var uninitializedContract))
                {
                    _uninitializedContracts[additionalContract] = uninitializedContract.Merge(additionalContract);
                }
                else
                {
                    _uninitializedContracts[additionalContract] = additionalContract;
                }
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

        foreach (var contractId in result.Contract.DependencyOf)
        {
            if (!Contracts.ContainsKey(contractId))
            {
                _emptyContracts.Add(contractId);
            }
        }
        
        // Check for Substitute contract
        if (result.Contract is not IHasStrings hasStrings)
        {
            return true;
        }

        var substitute = new Substitute(result.Contract);

        // remove old substitutes
        Contracts.Remove(substitute);

        var matches = new Dictionary<string, MatchCollection>();

        hasStrings.ApplyStringDecorator(s =>
            {
                var matchCollection = Substitute.InsertionRegex.Matches(s);
                if (matchCollection.Count > 0)
                {
                    matches[s] = matchCollection;
                }

                return s;
            }
        );

        if (matches.Count == 0)
        {
            _uninitializedContracts.Remove(substitute);
            return true;
        }

        _uninitializedContracts[substitute] = substitute with
        {
            Matches = matches
        };
        return true;
    }
}