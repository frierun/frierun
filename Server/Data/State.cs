using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public class State
{
    private readonly List<Application> _applications = [];
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
    [JsonIgnore]
    public IEnumerable<Contract> Contracts => _applications
        .SelectMany(application => application.Contracts.Values)
        .Concat(UnmanagedContracts);

    /// <summary>
    /// Gets contract by Guid.
    /// </summary>
    public TContract GetContract<TContract>(Guid id) where TContract : Contract =>
        (TContract)Contracts.Single(c => c.Id == id);

    /// <summary>
    /// Gets contract from the application by name.
    /// </summary>
    public TContract GetContract<TContract>(Application app, string name = "")
        where TContract : Contract
    {
        return GetContract<TContract>(app, new ContractRef<TContract>(name));
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
}