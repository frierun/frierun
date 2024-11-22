using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class ExecutionPlan
{
    private readonly ProviderRegistry _providerRegistry;
    private readonly Dictionary<ContractId, Contract> _contracts = new();
    private readonly Dictionary<ContractId, Resource> _resources = new();
    private readonly DirectedAcyclicGraph<ContractId> _graph = new();

    public ContractId RootContractId { get; }
    private Package Package => GetContract<Package>(RootContractId);
    public string Prefix => Package.Prefix ?? Package.Name;
    
    public State State { get; }

    public IReadOnlyDictionary<ContractId, Contract> Contracts => _contracts;

    public ExecutionPlan(Package package, State state, ProviderRegistry providerRegistry)
    {
        RootContractId = package.Id;
        State = state;
        _providerRegistry = providerRegistry;
        var queue = new Queue<Contract>();
        AddContract(package, queue);

        while (queue.Count > 0)
        {
            var contract = queue.Dequeue();

            foreach (var dependency in contract.Provider!.Dependencies(contract, this))
            {
                AddContract(dependency.Preceding, queue);
                AddContract(dependency.Following, queue);

                _graph.AddEdge(dependency.Preceding.Id, dependency.Following.Id);
            }
        }

        Initialize();
    }

    /// <summary>
    /// Adds contract and resolves the provider if needed.
    /// </summary>
    private void AddContract(Contract contract, Queue<Contract> queue)
    {
        if (_contracts.ContainsKey(contract.Id))
        {
            return;
        }

        _graph.AddVertex(contract.Id);

        if (contract.Provider == null)
        {
            var provider = _providerRegistry.Get(contract.ResourceType).FirstOrDefault();
            if (provider == null)
            {
                throw new Exception($"Can't find provider for resource {contract.ResourceType}");
            }

            contract = contract with
            {
                Provider = provider
            };
        }

        _contracts[contract.Id] = contract;
        queue.Enqueue(contract);
    }

    private Contract GetContract(ContractId contractId)
    {
        if (_resources.ContainsKey(contractId))
        {
            throw new Exception($"Resource {contractId} already installed");
        }

        return _contracts[contractId];
    }

    public T GetContract<T>(ContractId contractId)
        where T : Contract
    {
        // TODO: check if contractId is of type T
        return (T)GetContract(contractId);
    }

    public void UpdateContract(Contract contract)
    {
        if (_resources.ContainsKey(contract.Id))
        {
            throw new Exception($"Resource {contract.Id} already installed");
        }

        _contracts[contract.Id] = contract;
    }

    public IEnumerable<T> GetResourcesOfType<T>()
        where T : Contract
    {
        return _contracts.Values.OfType<T>();
    }
    
    /// <summary>
    /// Initializes all contracts in the execution plan.
    /// </summary>
    private void Initialize()
    {
        _graph.RunDfs(
            contractId =>
            {
                var contract = GetContract(contractId);
                UpdateContract(contract.Provider!.Initialize(contract, this));
            }
        );
    }


    /// <summary>
    /// Installs all contracts in the execution plan.
    /// </summary>
    public void Install()
    {
        _graph.RunDfs(
            contractId =>
            {
                var contract = GetContract(contractId);
                var resource = contract.Provider!.Install(contract, this);
                _resources[contract.Id] = resource;
                State.Resources.Add(resource);
            }
        );
    }

    public Resource GetResource(ContractId contractId)
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
        // TODO: check if contractId is of type T
        return (T)GetResource(contractId);
    }
}