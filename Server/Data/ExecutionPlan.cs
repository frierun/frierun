using System.Text.RegularExpressions;
using Frierun.Server.Services;

namespace Frierun.Server.Data;

public class ExecutionPlan : IExecutionPlan
{
    private readonly ProviderRegistry _providerRegistry;
    private readonly Dictionary<ContractId, Contract> _contracts = new();
    private readonly Dictionary<ContractId, Resource?> _resources = new();
    private readonly DirectedAcyclicGraph<ContractId> _graph = new();

    public ContractId<Package> RootContractId { get; }
    private Package Package => GetContract(RootContractId);
    public string Prefix => Package.Prefix ?? Package.Name;

    public State State { get; }

    public IReadOnlyDictionary<ContractId, Contract> Contracts => _contracts;

    public ExecutionPlan(Package package, State state, ProviderRegistry providerRegistry)
    {
        RootContractId = new ContractId<Package>(package.Name);
        State = state;
        _providerRegistry = providerRegistry;
        var queue = new Queue<Contract>();
        AddContract(package, queue);

        while (queue.Count > 0)
        {
            var contract = queue.Dequeue();

            foreach (var dependency in GetInstaller(contract).Dependencies(contract, this))
            {
                AddContract(dependency.Preceding, queue);
                AddContract(dependency.Following, queue);

                _graph.AddEdge(dependency.Preceding.Id, dependency.Following.Id);
            }
        }

        Initialize();
    }

    /// <summary>
    /// Gets installer for the contract.
    /// </summary>
    private IInstaller GetInstaller(Contract contract)
    {
        var installer = _providerRegistry.GetInstaller(contract.GetType(), contract.Installer);
        if (installer == null)
        {
            throw contract.Installer == null
                ? new Exception($"Can't find default installer for resource {contract.GetType()}")
                : new Exception($"Can't find installer '{contract.Installer}' for resource {contract.GetType()}");
        }

        return installer;
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

        // check installer and fill it if not set 
        var installer = GetInstaller(contract);
        if (contract.Installer != installer.GetType().Name)
        {
            contract = contract with
            {
                Installer = installer.GetType().Name
            };
        }

        _contracts[contract.Id] = contract;
        queue.Enqueue(contract);

        if (contract is IHasStrings hasStrings)
        {
            var matches = new Dictionary<string, MatchCollection>();

            hasStrings.ApplyStringDecorator(
                s =>
                {
                    var matchCollection = Substitute.InsertionRegex.Matches(s);
                    if (matchCollection.Count > 0)
                    {
                        matches[s] = matchCollection;
                    }

                    return s;
                }
            );

            if (matches.Count > 0)
            {
                AddContract(new Substitute(contract.Id, matches), queue);
            }
        }
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
                UpdateContract(GetInstaller(contract).Initialize(contract, this));
            }
        );
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
                var resource = GetInstaller(contract).Install(contract, this);
                _resources[contract.Id] = resource;
            }
        );

        foreach (var resource in _resources.Values.OfType<Resource>())
        {
            State.AddResource(resource);
        }
        
        return GetResource<Application>(RootContractId);
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
        // TODO: check if contractId is of type T
        var resource = GetResource(contractId);
        if (resource == null)
        {
            throw new Exception($"Contract {contractId} didn't install resource");
        }

        return (T)resource;
    }
}