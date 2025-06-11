using System.Diagnostics;

namespace Frierun.Server.Data;

public class ExecutionPlan : IExecutionPlan
{
    private readonly Dictionary<ContractId, Contract> _contracts;
    private readonly DirectedAcyclicGraph<ContractId> _graph = new();
    private readonly HashSet<Application> _requiredApplications = [];

    public IReadOnlyDictionary<ContractId, Contract> Contracts => _contracts;

    public ExecutionPlan(
        Dictionary<ContractId, Contract> contracts
    )
    {
        _contracts = contracts;

        BuildGraph();
    }

    /// <summary>
    /// Builds the graph of contracts.
    /// </summary>
    private void BuildGraph()
    {
        foreach (var contract in _contracts.Values)
        {
            _graph.AddVertex(contract.Id);
        }

        foreach (var contract in _contracts.Values)
        {
            foreach (var dependency in contract.DependsOn)
            {
                _graph.AddEdge(dependency, contract);
            }

            foreach (var dependency in contract.DependencyOf)
            {
                _graph.AddEdge(contract, dependency);
            }
        }
    }

    /// <summary>
    /// Get contract by id.
    /// </summary>
    public Contract GetContract(ContractId contractId)
    {
        return _contracts[contractId];
    }

    /// <summary>
    /// Get contract by id.
    /// </summary>
    public T GetContract<T>(ContractId<T> contractId)
        where T : Contract
    {
        return (T)GetContract((ContractId)contractId);
    }

    public void UpdateContract(Contract contract)
    {
        Debug.Assert(
            !_contracts.TryGetValue(contract, out var existing) || !existing.Installed,
            $"Contract is already installed"
        );

        _contracts[contract] = contract;
    }

    /// <summary>
    /// Installs all contracts in the execution plan.
    /// </summary>
    public Application Install()
    {
        var installedContracts = new List<Contract>();
        _graph.RunDfs(
            contractId =>
            {
                var contract = GetContract(contractId);
                Debug.Assert(!contract.Installed);

                var installedContract = contract.Install(this);
                
                Debug.Assert(installedContract.Id == contractId);

                if (installedContract is not Package)
                {
                    installedContracts.Add(installedContract);
                }
                _contracts[contractId] = installedContract;

                var handlerApplication = installedContract.Handler?.Application;
                if (handlerApplication != null)
                {
                    _requiredApplications.Add(handlerApplication);
                }
            }
        );

        var application = _contracts.Values.OfType<Package>().First().Result;
        Debug.Assert(application != null);

        return new Application
        {
            Name = application.Name,
            Package = application.Package,
            Description = application.Description,
            Url = application.Url,
            Contracts = installedContracts,
            RequiredApplications = _requiredApplications.Select(app => app.Name).ToList()
        };
    }
}