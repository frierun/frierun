using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(Container), nameof(Container))]
[JsonDerivedType(typeof(Dependency), nameof(Dependency))]
[JsonDerivedType(typeof(File), nameof(File))]
[JsonDerivedType(typeof(HttpEndpoint), nameof(HttpEndpoint))]
[JsonDerivedType(typeof(Mount), nameof(Mount))]
[JsonDerivedType(typeof(Network), nameof(Network))]
[JsonDerivedType(typeof(Package), nameof(Package))]
[JsonDerivedType(typeof(Parameter), nameof(Parameter))]
[JsonDerivedType(typeof(Password), nameof(Password))]
[JsonDerivedType(typeof(PortEndpoint), nameof(PortEndpoint))]
[JsonDerivedType(typeof(Substitute), nameof(Substitute))]
[JsonDerivedType(typeof(Volume), nameof(Volume))]
public abstract record Contract(
    string Name,
    string? Installer = null
)
{
    [JsonIgnore] public ContractId Id => new(GetType(), Name);
    
    public virtual Contract With(Contract other)
    {
        throw new Exception("Not implemented");
    }
}