using System.Text.Json.Serialization;

namespace Frierun.Server.Resources;

public record ResourceDefinition<T>() : ResourceDefinition(typeof(T))
    where T: Resource;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(ContainerDefinition), "Container")]
[JsonDerivedType(typeof(HttpEndpointDefinition), "HttpEndpoint")]
[JsonDerivedType(typeof(VolumeDefinition), "Volume")]
public abstract record ResourceDefinition([property: JsonIgnore]Type ResourceType);
