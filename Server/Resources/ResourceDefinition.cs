using System.Text.Json.Serialization;

namespace Frierun.Server.Resources;

public record ResourceDefinition<T>(IReadOnlyList<ResourceDefinition> Children) : ResourceDefinition(Guid.NewGuid(), typeof(T), Children)
    where T: Resource;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(ContainerDefinition), "Container")]
[JsonDerivedType(typeof(HttpEndpointDefinition), "HttpEndpoint")]
[JsonDerivedType(typeof(VolumeDefinition), "Volume")]
public abstract record ResourceDefinition(
    Guid Id,
    [property: JsonIgnore] Type ResourceType,
    IReadOnlyList<ResourceDefinition>? Children)
{
    public IReadOnlyList<ResourceDefinition> Children { get; } = Children ?? new List<ResourceDefinition>();
}
