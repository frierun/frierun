using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class ExecutionPlan : IExecutionPlan
{
    private readonly InstallerRegistry _installerRegistry;
    private readonly Dictionary<ContractId, Contract> _contracts;
    private readonly OrderedDictionary<ContractId, Resource?> _resources = new();
    private readonly DirectedAcyclicGraph<ContractId> _graph = new();

    private ContractId<Package> RootContractId { get; }

    private State State { get; }

    public IReadOnlyDictionary<ContractId, Contract> Contracts => _contracts;

    public ExecutionPlan(
        Package package,
        Dictionary<ContractId, Contract> contracts,
        State state,
        InstallerRegistry installerRegistry
    )
    {
        _contracts = contracts;
        RootContractId = new ContractId<Package>(package.Name);
        State = state;
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

        var resources = _resources.Values
            .OfType<Resource>()
            .Where(resource => resource is not Application)
            .ToList();
        var application = GetResource<Application>(RootContractId) with { DependsOn = resources };

        State.AddResource(application);
        foreach (var resource in resources)
        {
            State.AddResource(resource);
        }

        return application;
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

    /// <summary>
    /// Gets all resources that are prerequisites for the given contract.
    /// If a required contract doesn't emit resource, get its prerequisites recursively.
    /// </summary>
    public IEnumerable<Resource> GetDependentResources(ContractId contractId)
    {
        var visited = new HashSet<ContractId>();
        var queue = new Queue<ContractId>();

        queue.Enqueue(contractId);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            foreach (var prerequisite in _graph.GetPrerequisites(current))
            {
                if (!visited.Add(prerequisite))
                {
                    continue;
                }
                
                var resource = GetResource(prerequisite);
                if (resource != null)
                {
                    yield return resource;
                }
                else
                {
                    queue.Enqueue(prerequisite);
                }
            }
        }
    }
}