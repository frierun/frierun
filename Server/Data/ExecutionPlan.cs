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
    /// Gets contract by id.
    /// </summary>
    public Contract GetContract(ContractId contractId)
    {
        var contract = _contracts[contractId];
        Debug.Assert(!contract.Installed);

        return _contracts[contractId];
    }

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
                Debug.Assert(
                    installedContract is not IHasResult { Result: null },
                    "Result must be set for installed contract"
                );

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

    public Resource GetResource(ContractId contractId)
    {
        return GetResource<Resource>(contractId);
    }

    public T GetResource<T>(ContractId contractId)
        where T : Resource
    {
        var contract = _contracts[contractId];
        Debug.Assert(contract.Installed, $"Contract is not installed");
        Debug.Assert(contract is IHasResult, $"Contract does not have a result");

        var result = ((IHasResult)contract).Result;
        Debug.Assert(result is T, $"Contract result is of wrong type");

        return (T)result;
    }
}