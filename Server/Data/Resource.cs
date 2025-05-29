using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(GenericHttpEndpoint), nameof(GenericHttpEndpoint))]
[JsonDerivedType(typeof(TraefikHttpEndpoint), nameof(TraefikHttpEndpoint))]
public abstract class Resource
{
}
