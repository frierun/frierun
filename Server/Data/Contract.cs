using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
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
    bool Installed = false,
    IEnumerable<ContractId>? DependsOn = null,
    IEnumerable<ContractId>? DependencyOf = null,
    Lazy<IHandler?>? LazyHandler = null
)
{
    [JsonIgnore] public ContractId Id => ContractId.Create(GetType(), Name);

    [JsonIgnore] public IEnumerable<ContractId> DependsOn { get; init; } = DependsOn ?? Array.Empty<ContractId>();
    [JsonIgnore] public IEnumerable<ContractId> DependencyOf { get; init; } = DependencyOf ?? Array.Empty<ContractId>();

    [JsonPropertyName("Handler")]
    [JsonInclude]
    protected Lazy<IHandler?> LazyHandler { get; init; } = LazyHandler ?? new Lazy<IHandler?>((IHandler?)null);

    [JsonIgnore]
    public IHandler? Handler
    {
        get => LazyHandler.Value;
        init => LazyHandler = new Lazy<IHandler?>(value);
    }

    public virtual Contract With(Contract other)
    {
        throw new Exception("Not implemented");
    }

    public static implicit operator ContractId(Contract contract) => contract.Id;

    public Contract Install(ExecutionPlan plan)
    {
        if (Handler == null)
        {
            throw new Exception($"No handler for {Name}");
        }

        return Handler.Install(this, plan) with {Installed = true};
    }
    
    public void Uninstall()
    {
        Handler?.Uninstall(this);
    }    
}