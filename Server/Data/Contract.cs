using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(typeof(ConnectExternalContainer), nameof(ConnectExternalContainer))]
[JsonDerivedType(typeof(Container), nameof(Container))]
[JsonDerivedType(typeof(Dependency), nameof(Dependency))]
[JsonDerivedType(typeof(Domain), nameof(Domain))]
[JsonDerivedType(typeof(File), nameof(File))]
[JsonDerivedType(typeof(HttpEndpoint), nameof(HttpEndpoint))]
[JsonDerivedType(typeof(Mount), nameof(Mount))]
[JsonDerivedType(typeof(Mysql), nameof(Mysql))]
[JsonDerivedType(typeof(Network), nameof(Network))]
[JsonDerivedType(typeof(Package), nameof(Package))]
[JsonDerivedType(typeof(Parameter), nameof(Parameter))]
[JsonDerivedType(typeof(Password), nameof(Password))]
[JsonDerivedType(typeof(PortEndpoint), nameof(PortEndpoint))]
[JsonDerivedType(typeof(Postgresql), nameof(Postgresql))]
[JsonDerivedType(typeof(Redis), nameof(Redis))]
[JsonDerivedType(typeof(Selector), nameof(Selector))]
[JsonDerivedType(typeof(Substitute), nameof(Substitute))]
[JsonDerivedType(typeof(Volume), nameof(Volume))]
public abstract record Contract(
    string Name,
    InstallerDefinition? Installer = null,
    IEnumerable<ContractId>? DependsOn = null,
    IEnumerable<ContractId>? DependencyOf = null
)
{
    [JsonIgnore] public ContractId Id => ContractId.Create(GetType(), Name);

    [JsonIgnore] public IEnumerable<ContractId> DependsOn { get; init; } = DependsOn ?? Array.Empty<ContractId>();
    [JsonIgnore] public IEnumerable<ContractId> DependencyOf { get; init; } = DependencyOf ?? Array.Empty<ContractId>();

    public virtual Contract With(Contract other)
    {
        throw new Exception("Not implemented");
    }

    public static implicit operator ContractId(Contract contract) => contract.Id;
}