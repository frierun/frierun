using System.Text.Json.Serialization;
using Frierun.Server.Resources;

namespace Frierun.Server.Models;

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
}