using System.Text.Json.Serialization;
using Frierun.Server.Models;

namespace Frierun.Server.Resources;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(Application), "Application")]
[JsonDerivedType(typeof(Container), "Container")]
[JsonDerivedType(typeof(ContainerGroup), "ContainerGroup")]
[JsonDerivedType(typeof(File), "File")]
[JsonDerivedType(typeof(HttpEndpoint), "HttpEndpoint")]
[JsonDerivedType(typeof(PortHttpEndpoint), "PortHttpEndpoint")]
[JsonDerivedType(typeof(TraefikHttpEndpoint), "TraefikHttpEndpoint")]
[JsonDerivedType(typeof(Volume), "Volume")]
public abstract record Resource
{
    private readonly List<Resource> _dependsOn = [];
    private readonly List<Resource> _requiredBy = [];
    private readonly List<Guid> _dependsOnIds = [];
    private bool _hydrated = true;
    
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// Enumerates all resources in the hierarchy.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<Resource> AllDependencies => DependsOn.SelectMany(resource => resource.AllDependencies).Prepend(this);
    
    /// <summary>
    /// Gets the resources that require this resource.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Resource> RequiredBy => _requiredBy;

    /// <summary>
    /// Gets the resources that this resource depends on.
    /// </summary>
    [JsonIgnore]
    public IReadOnlyList<Resource> DependsOn
    {
        get
        {
            if (!_hydrated)
            {
                throw new InvalidOperationException("Resource has not been hydrated yet.");
            }
            return _dependsOn;
        }
        init
        {
            if (_dependsOnIds.Count > 0)
            {
                throw new InvalidOperationException("Only one of DependsOn and DependsOnIds can be set.");
            }
            
            foreach (var resource in value)
            {
                _dependsOn.Add(resource);
                _dependsOnIds.Add(resource.Id);
                resource._requiredBy.Add(this);
            }
        }
    }

    /// <summary>
    /// Dependent resource IDs. Used for serialization.
    /// </summary>
    public IReadOnlyList<Guid> DependsOnIds
    {
        get => _dependsOnIds;
        init
        {
            if (_dependsOn.Count > 0)
            {
                throw new InvalidOperationException("Only one of DependsOn and DependsOnIds can be set.");
            }
            
            foreach (var id in value)
            {
                _dependsOnIds.Add(id);
            }

            _hydrated = false;
        }
    }

    /// <summary>
    /// Hydrates the resource with the state.
    /// </summary>
    public void Hydrate(State state)
    {
        if (_hydrated)
        {
            return;
        }

        foreach (var guid in _dependsOnIds)
        {
            if (state.FindResource(guid) is not { } resource)
            {
                throw new InvalidOperationException($"Resource {guid} not found.");
            }
            _dependsOn.Add(resource);
            resource._requiredBy.Add(this);
        }

        _hydrated = true;
    }

    /// <summary>
    /// Uninstalls the resource.
    /// </summary>
    public void Uninstall()
    {
        if (RequiredBy.Count > 0)
        {
            throw new InvalidOperationException("Resource is required by other resources.");
        }

        foreach (var resource in _dependsOn)
        {
            resource._requiredBy.Remove(this);
        }
    }
}