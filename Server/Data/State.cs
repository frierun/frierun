using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public class State : IJsonOnDeserialized
{
    private readonly IList<Resource> _resources = [];
    public event Action<Resource> ResourceAdded = _ => { }; 
    public event Action<Resource> ResourceRemoved = _ => { }; 

    public IEnumerable<Resource> Resources
    {
        get => _resources;
        init => _resources = new List<Resource>(value);
    }
    
    /// <inheritdoc />
    public void OnDeserialized()
    {
        foreach (var resource in Resources)
        {
            resource.Hydrate(this);
        }
    }
    
    /// <summary>
    /// Gets a resource by its ID.
    /// </summary>
    public Resource? FindResource(Guid id)
    {
        return Resources.FirstOrDefault(resource => resource.Id == id);
    }
    
    /// <summary>
    /// Adds a resource to the state if it does not already exist.
    /// </summary>
    public void AddResource(Resource resource)
    {
        if (FindResource(resource.Id) != null)
        {
            return;
        }
        
        _resources.Add(resource);
        ResourceAdded(resource);
    }

    /// <summary>
    /// Removes a resource from the state.
    /// </summary>
    public void RemoveResource(Resource resource)
    {
        _resources.Remove(resource);
        ResourceRemoved(resource);
    }
}