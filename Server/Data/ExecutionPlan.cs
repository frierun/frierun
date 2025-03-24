using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class ExecutionPlan : IExecutionPlan
{
    private readonly InstallerRegistry _installerRegistry;
    private readonly Dictionary<ContractId, Contract> _contracts;
    private readonly OrderedDictionary<ContractId, Resource?> _resources = new();
    private readonly DirectedAcyclicGraph<ContractId> _graph = new();
    private readonly HashSet<Application> _requiredApplications = new();

    public IReadOnlyDictionary<ContractId, Contract> Contracts => _contracts;

    public ExecutionPlan(
        Dictionary<ContractId, Contract> contracts,
        InstallerRegistry installerRegistry
    )
    {
        _contracts = contracts;
        _installerRegistry = installerRegistry;

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
            
            foreach (var dependency in GetInstaller(contract).GetDependencies(contract, this))
            {
                _graph.AddEdge(dependency.Preceding, dependency.Following);
            }
        }
    }

    /// <summary>
    /// Gets installer for the contract.
    /// </summary>
    private IInstaller GetInstaller(Contract contract)
    {
        return _installerRegistry.GetInstallers(contract.GetType(), contract.Installer).First();
    }

    /// <summary>
    /// Gets contract by id.
    /// </summary>
    public Contract GetContract(ContractId contractId)
    {
        if (_resources.ContainsKey(contractId))
        {
            throw new Exception($"Resource {contractId} already installed");
        }

        return _contracts[contractId];
    }

    public T GetContract<T>(ContractId<T> contractId)
        where T : Contract
    {
        return (T)GetContract((ContractId)contractId);
    }

    public void UpdateContract(Contract contract)
    {
        if (_resources.ContainsKey(contract))
        {
            throw new Exception($"Resource {contract.Id} already installed");
        }

        _contracts[contract] = contract;
    }

    /// <summary>
    /// Adds an application that is required for the final result
    /// </summary>
    public void RequireApplication(Application application)
    {
        _requiredApplications.Add(application);
    }

    /// <summary>
    /// Installs all contracts in the execution plan.
    /// </summary>
    public Application Install()
    {
        _graph.RunDfs(
            contractId =>
            {
                var contract = GetContract(contractId);
                if (_resources.ContainsKey(contractId))
                {
                    throw new Exception($"Resource {contractId} already installed");
                }
                var resource = GetInstaller(contract).Install(contract, this);
                _resources[contract] = resource;
            }
        );

        var application = _resources.Values.OfType<Application>().First();
        return application with
        {
            Resources = _resources.Values
                .OfType<Resource>()
                .Where(resource => resource is not Application)
                .ToList(),
            RequiredApplications = _requiredApplications.Select(app => app.Name).ToList()
        };
    }

    public Resource? GetResource(ContractId contractId)
    {
        if (!_resources.TryGetValue(contractId, out var resource))
        {
            throw new Exception($"Contract {contractId} not installed");
        }

        return resource;
    }

    public T GetResource<T>(ContractId contractId)
        where T : Resource
    {
        var resource = GetResource(contractId);
        if (resource == null)
        {
            throw new Exception($"Contract {contractId} didn't install resource");
        }

        return (T)resource;
    }
}