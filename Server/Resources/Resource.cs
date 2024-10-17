using System.Text.Json.Serialization;

namespace Frierun.Server.Resources;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(Container), "Container")]
[JsonDerivedType(typeof(HttpEndpoint), "HttpEndpoint")]
[JsonDerivedType(typeof(Volume), "Volume")]
public abstract record Resource(Guid Id);
