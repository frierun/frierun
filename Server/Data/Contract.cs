using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Frierun.Server.Handlers;
using Swashbuckle.AspNetCore.Annotations;

namespace Frierun.Server.Data;

public abstract record Contract<THandler> : Contract
    where THandler : IHandler
{
    [JsonIgnore]
    public new THandler? Handler
    {
        get => (THandler?)LazyHandler.Value;
        init => LazyHandler = new Lazy<IHandler?>(value);
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(Application), nameof(Application))]
[JsonDerivedType(typeof(CloudflareApiConnection), nameof(CloudflareApiConnection))]
[JsonDerivedType(typeof(CloudflareTunnel), nameof(CloudflareTunnel))]
[JsonDerivedType(typeof(Container), nameof(Container))]
[JsonDerivedType(typeof(Daemon), nameof(Daemon))]
[JsonDerivedType(typeof(DockerApiConnection), nameof(DockerApiConnection))]
[JsonDerivedType(typeof(Domain), nameof(Domain))]
[JsonDerivedType(typeof(File), nameof(File))]
[JsonDerivedType(typeof(HttpEndpoint), nameof(HttpEndpoint))]
[JsonDerivedType(typeof(Mysql), nameof(Mysql))]
[JsonDerivedType(typeof(Network), nameof(Network))]
[JsonDerivedType(typeof(Optional), nameof(Optional))]
[JsonDerivedType(typeof(Parameter), nameof(Parameter))]
[JsonDerivedType(typeof(Password), nameof(Password))]
[JsonDerivedType(typeof(PortEndpoint), nameof(PortEndpoint))]
[JsonDerivedType(typeof(Postgresql), nameof(Postgresql))]
[JsonDerivedType(typeof(Redis), nameof(Redis))]
[JsonDerivedType(typeof(Selector), nameof(Selector))]
[JsonDerivedType(typeof(SshConnection), nameof(SshConnection))]
[JsonDerivedType(typeof(Volume), nameof(Volume))]
[SwaggerSchema(Required = ["type"])]
public abstract record Contract
{
    [MemberNotNullWhen(true, nameof(Id), nameof(Handler))]
    public virtual bool Installed => Id != Guid.Empty;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; init; } = Guid.Empty;

    public IEnumerable<ContractId> DependsOn { get; init; } = [];

    [JsonPropertyName("handler")]
    [JsonInclude]
    public Lazy<IHandler?> LazyHandler { get; protected init; } = new((IHandler?)null);

    [JsonIgnore]
    public IHandler? Handler
    {
        get => LazyHandler.Value;
        init => LazyHandler = new Lazy<IHandler?>(value);
    }

    [JsonIgnore] public string? HandlerApplication { get; init; }


    /// <summary>
    /// Transforms all arguments using the transformer.
    /// </summary>
    public virtual Contract Transform(IArgumentTransformer transformer)
    {
        return this with
        {
            DependsOn = DependsOn.Select(transformer.Transform).ToArray()
        };
    }

    /// <summary>
    /// Resolves all arguments and returns resolved contract
    /// </summary>
    public Contract ResolveArguments(ExecutionPlan plan)
    {
        return Transform(new ContractResolver(plan));
    }

    /// <summary>
    /// Gets all arguments used by the contract.
    /// </summary>
    public IEnumerable<IArgument> GetArguments()
    {
        var counter = new ArgumentCounter();
        Transform(counter);
        return counter.Arguments;
    }

    public IEnumerable<ContractId> GetDependencies()
    {
        return GetArguments()
            .SelectMany(argument => argument.RequiredContracts)
            .Distinct();
    }

    /// <summary>
    /// Merges contracts restrictions of the same type 
    /// </summary>
    public abstract Contract Merge(Contract other);

    /// <summary>
    /// Checks if this installed contract is fulfilling the other contract.
    /// </summary>
    public virtual bool IsSubset(Contract other)
    {
        return false;
    }

    /// <summary>
    /// Installs the contract using the handler.
    /// </summary>
    public Contract Install(ExecutionPlan plan)
    {
        Debug.Assert(!Installed);
        Debug.Assert(LazyHandler.Value != null, "Handler must be initialized");
        return LazyHandler.Value.Install(this, plan) with { Id = Guid.CreateVersion7() };
    }

    /// <summary>
    /// Uninstalls the contract
    /// </summary>
    public void Uninstall()
    {
        Debug.Assert(Installed, "Contract is not installed");
        LazyHandler.Value?.Uninstall(this);
    }
}