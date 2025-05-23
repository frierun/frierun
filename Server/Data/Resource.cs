using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(DockerPortEndpoint), nameof(DockerPortEndpoint))]
[JsonDerivedType(typeof(DockerVolume), nameof(DockerVolume))]
[JsonDerivedType(typeof(GenericHttpEndpoint), nameof(GenericHttpEndpoint))]
[JsonDerivedType(typeof(LocalPath), nameof(LocalPath))]
[JsonDerivedType(typeof(TraefikHttpEndpoint), nameof(TraefikHttpEndpoint))]
public abstract class Resource
{
}
