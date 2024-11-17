using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class ExecutionPlan
{
    private readonly ProviderRegistry _providerRegistry;
    private readonly Dictionary<ContractId, Contract> _contracts = new();
    private readonly Dictionary<ContractId, Resource> _resources = new();
    private readonly List<ContractDependency> _dependencies = new();

    public Package Package { get; }
    public string Prefix => Package.Name;
    public State State { get; }
    
    public IReadOnlyDictionary<ContractId, Contract> Contracts => _contracts;
    
    public ExecutionPlan(Package package, State state, ProviderRegistry providerRegistry)
    {
        Package = package;
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

                _dependencies.Add(dependency);
            }
        }
    }

    /// <summary>
    /// Adds contract and resolves the provider if needed.
    /// </summary>
    private void AddContract(Contract contract, Queue<Contract> queue)
    {
        if (HasContract(contract.Id))
        {
            return;
        }

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
    
    public bool HasContract(ContractId contractId)
    {
        return _contracts.ContainsKey(contractId);
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
        where T: Contract
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
        where T: Contract
    {
        return _contracts.Values.OfType<T>();
    }
    
    public string GetPrefixedName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return Prefix;
        }
        return $"{Prefix}-{name}";
    }

    /// <summary>
    /// Installs all contracts in the execution plan.
    /// </summary>
    public void Install()
    {
        var visited = new HashSet<ContractId>();
        
        foreach (var contract in _contracts.Values)
        {
            InstallRecursive(contract.Id, visited);
        }
    }
    
    /// <summary>
    /// Recursively installs the contract and its dependencies.
    /// </summary>
    private void InstallRecursive(ContractId contractId, HashSet<ContractId> visited)
    {
        if (_resources.ContainsKey(contractId))
        {
            return;
        }
        
        if (!visited.Add(contractId))
        {
            throw new Exception("Circular dependency detected");
        }

        foreach (var dependency in _dependencies.Where(dependency => dependency.Following.Id == contractId))
        {
            InstallRecursive(dependency.Preceding.Id, visited);
        }

        var contract = GetContract(contractId);
        var resource = contract.Provider!.Install(contract, this);
        _resources[contractId] = resource;
        State.Resources.Add(resource);
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
        where T:Resource
    {
        // TODO: check if contractId is of type T
        return (T)GetResource(contractId);
    }

    
}