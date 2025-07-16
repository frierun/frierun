using System.Diagnostics;
using System.Text.RegularExpressions;
using Frierun.Server.Handlers;

namespace Frierun.Server.Data;

public class DiscoveryGraph
{
    private readonly HashSet<ContractId> _emptyContracts = new();
    private readonly Dictionary<ContractId, Contract> _uninitializedContracts = new();
    public Dictionary<ContractId, Contract> Contracts { get; } = new();

    public DiscoveryGraph()
    {
    }

    public DiscoveryGraph(DiscoveryGraph graph)
    {
        Contracts = new Dictionary<ContractId, Contract>(graph.Contracts);
        _emptyContracts = [..graph._emptyContracts];
        _uninitializedContracts = new Dictionary<ContractId, Contract>(graph._uninitializedContracts);
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
            var (contractId, contract) = _uninitializedContracts.First();
            _uninitializedContracts.Remove(contractId);
            Debug.Assert(!Contracts.ContainsKey(contractId));

            return (contractId, contract);
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
    /// Saves contract as initialized
    /// </summary>
    private void SetInitializedContract(Contract contract)
    {
        Debug.Assert(contract.Handler != null, "Initialized contract must have a handler");

        Contracts[contract] = contract;

        if (contract is not IHasStrings hasStrings)
        {
            return;
        }

        var substitute = new Substitute(contract);

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
            return;
        }

        _uninitializedContracts[substitute] = substitute with
        {
            Matches = matches
        };
    }

    /// <summary>
    /// Applies contract initialization result.
    /// </summary>
    public void Apply(ContractInitializeResult result)
    {
        SetInitializedContract(result.Contract);

        foreach (var additionalContract in result.AdditionalContracts)
        {
            if (Contracts.TryGetValue(additionalContract, out var initializedContract))
            {
                SetInitializedContract(initializedContract.Merge(additionalContract));
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
    }
}