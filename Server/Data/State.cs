using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public class State
{
    private readonly IList<Application> _applications = [];
    public event Action<Application> ApplicationAdded = _ => { }; 
    public event Action<Application> ApplicationRemoved = _ => { }; 

    [JsonIgnore]
    public IEnumerable<Contract> Contracts => _applications.SelectMany(application => application.Contracts);
    
    public IEnumerable<Application> Applications
    {
        get => _applications;
        init => _applications = new List<Application>(value);
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