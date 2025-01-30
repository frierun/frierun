using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record State : IJsonOnDeserialized
{
    public IList<Resource> Resources { get; init; } = new List<Resource>();

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
    
    public void AddResource(Resource resource)
    {
        if (FindResource(resource.Id) != null)
        {
            return;
        }
        
        Resources.Add(resource);
    }
}