using System.Diagnostics;
using System.Text.Json.Serialization;
using Frierun.Server.Handlers;

namespace Frierun.Server.Data;

public abstract record Contract<THandler>(
    string Name,
    bool Installed = false,
    IEnumerable<ContractId>? DependsOn = null,
    IEnumerable<ContractId>? DependencyOf = null,
    Lazy<IHandler?>? LazyHandler = null)
    : Contract(Name, Installed, DependsOn, DependencyOf, LazyHandler) where THandler : IHandler
{
    [JsonIgnore]
    public new THandler? Handler
    {
        get => (THandler?)LazyHandler.Value;
        init => LazyHandler = new Lazy<IHandler?>(value);
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(CloudflareApiConnection), nameof(CloudflareApiConnection))]
[JsonDerivedType(typeof(CloudflareTunnel), nameof(CloudflareTunnel))]
[JsonDerivedType(typeof(Container), nameof(Container))]
[JsonDerivedType(typeof(Dependency), nameof(Dependency))]
[JsonDerivedType(typeof(DockerApiConnection), nameof(DockerApiConnection))]
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

    [JsonPropertyName("handler")]
    [JsonInclude]
    public Lazy<IHandler?> LazyHandler { get; protected init; } = LazyHandler ?? new Lazy<IHandler?>((IHandler?)null);

    [JsonIgnore]
    public IHandler? Handler
    {
        get => LazyHandler.Value;
        init => LazyHandler = new Lazy<IHandler?>(value);
    }

    public virtual bool Installed { get; init; } = Installed;


    public static implicit operator ContractId(Contract contract) => contract.Id;

    /// <summary>
    /// Installs the contract using the handler.
    /// </summary>
    public Contract Install(ExecutionPlan plan)
    {
        if (Handler == null)
        {
            throw new Exception($"No handler for {Name}");
        }

        return Handler.Install(this, plan) with { Installed = true };
    }

    /// <summary>
    /// Uninstalls the contract
    /// </summary>
    public void Uninstall()
    {
        Debug.Assert(Installed, "Contract is not installed");
        Handler?.Uninstall(this);
    }
}