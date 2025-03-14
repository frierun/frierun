using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(Application), nameof(Application))]
[JsonDerivedType(typeof(DockerContainer), nameof(DockerContainer))]
[JsonDerivedType(typeof(DockerNetwork), nameof(DockerNetwork))]
[JsonDerivedType(typeof(DockerPortEndpoint), nameof(DockerPortEndpoint))]
[JsonDerivedType(typeof(DockerVolume), nameof(DockerVolume))]
[JsonDerivedType(typeof(GenericHttpEndpoint), nameof(GenericHttpEndpoint))]
[JsonDerivedType(typeof(GeneratedPassword), nameof(GeneratedPassword))]
[JsonDerivedType(typeof(ResolvedParameter), nameof(ResolvedParameter))]
[JsonDerivedType(typeof(TraefikHttpEndpoint), nameof(TraefikHttpEndpoint))]
[JsonDerivedType(typeof(MysqlDatabase), nameof(MysqlDatabase))]
[JsonDerivedType(typeof(PostgresqlDatabase), nameof(PostgresqlDatabase))]
[JsonDerivedType(typeof(RedisDatabase), nameof(RedisDatabase))]
public abstract record Resource
{
    private readonly List<Resource> _dependsOn = [];
    private readonly List<Guid> _dependsOnIds = [];
    private bool _hydrated = true;
    
    public Guid Id { get; init; } = Guid.NewGuid();
    
    /// <summary>
    /// Enumerates all resources for this application.
    /// </summary>
    [JsonIgnore]
    public IEnumerable<Resource> AllDependencies => DependsOn
        .SelectMany(resource => resource.AllDependencies)
        .Where(resource => resource.GetType() != typeof(Application))
        .Prepend(this);
    
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
            if (_dependsOnIds is { Count: > 0 })
            {
                throw new InvalidOperationException("Only one of DependsOn and DependsOnIds can be set.");
            }
            
            foreach (var resource in value)
            {
                _dependsOn.Add(resource);
                _dependsOnIds.Add(resource.Id);
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
            if (_dependsOn is { Count: > 0 })
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
        }

        _hydrated = true;
    }
}