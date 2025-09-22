using System.Diagnostics;

namespace Frierun.Server.Data;

public class State
{
    private readonly List<Application> _applications = [];
    private readonly Dictionary<Guid, Contract> _contracts = new();
    
    public event Action<Application> ApplicationAdded = _ => { };
    public event Action<Application> ApplicationRemoved = _ => { };

    public List<Contract> UnmanagedContracts { get; init; } = [];

    public IEnumerable<Application> Applications
    {
        get => _applications;
        init => _applications = [..value];
    }

    /// <summary>
    /// Lists all installed contracts
    /// </summary>
    public IReadOnlyDictionary<Guid, Contract> ContractsById
    {
        get => _contracts;
        init => _contracts = new Dictionary<Guid, Contract>(value);
    }

    /// <summary>
    /// Gets contract by Guid.
    /// </summary>
    public TContract GetContract<TContract>(Guid id) where TContract : Contract => (TContract)_contracts[id];
    public Contract GetContract(Guid id) => _contracts[id];

    /// <summary>
    /// Gets contract from the application by name.
    /// </summary>
    public TContract GetContract<TContract>(Application app, string name = "")
        where TContract : Contract
    {
        return GetContract(app, new ContractRef<TContract>(name));
    }
    
    /// <summary>
    /// Gets contract from the application by name.
    /// </summary>
    public TContract GetContract<TContract>(Application app, ContractRef<TContract> contractRef)
        where TContract : Contract
    {
        var guid = app.ContractRefs[contractRef];
        return GetContract<TContract>(guid);
    }

    /// <summary>
    /// Gets a contract list by type.
    /// </summary>
    public IEnumerable<TContract> GetContracts<TContract>() where TContract : Contract
    {
        return _contracts.Values.OfType<TContract>();
    }
    

    /// <summary>
    /// Adds a newly installed application to the state.
    /// </summary>
    public void AddApplication(Application application)
    {
        _applications.Add(application);
        ApplicationAdded(application);
    }

    /// <summary>
    /// Removes an application from the state.
    /// </summary>
    public void RemoveApplication(Application application)
    {
        _applications.Remove(application);
        ApplicationRemoved(application);
    }

    /// <summary>
    /// Adds a newly installed contract to the state.
    /// </summary>
    public void AddContract(Contract contract)
    {
        Debug.Assert(contract.Id != null);
        _contracts[(Guid)contract.Id] = contract;
    }
    
    /// <summary>
    /// Removes a contract from the state.
    /// </summary>
    public void RemoveContract(Contract contract)
    {
        Debug.Assert(contract.Id != null);
        _contracts.Remove((Guid)contract.Id);
    }    
}