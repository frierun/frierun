using System.Text.Json.Serialization;

namespace Frierun.Server.Resources;

public record ResourceDefinition<T>(
    IReadOnlyList<ResourceDefinition> Children,
    string? Name
) : ResourceDefinition(typeof(T), Children, Name)
    where T : Resource;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(ContainerDefinition), "Container")]
[JsonDerivedType(typeof(ContainerGroupDefinition), "ContainerGroup")]
[JsonDerivedType(typeof(FileDefinition), "File")]
[JsonDerivedType(typeof(HttpEndpointDefinition), "HttpEndpoint")]
[JsonDerivedType(typeof(VolumeDefinition), "Volume")]
public abstract record ResourceDefinition(
    [property: JsonIgnore] Type ResourceType,
    IReadOnlyList<ResourceDefinition>? Children,
    string? Name
)
{
    public IReadOnlyList<ResourceDefinition> Children { get; } = Children ?? new List<ResourceDefinition>();
}