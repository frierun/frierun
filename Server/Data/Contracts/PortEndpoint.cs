using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record PortEndpoint(
    Protocol Protocol,
    int Port,
    string? Name = null,
    ContractId<Container>? Container = null,
    int ExternalPort = 0,
    string? ExternalIp = null
) : Contract(Name ?? $"{(Container != null ? Container.Name + ":" : "")}{Port}/{Protocol}")
{
    [MemberNotNullWhen(true, nameof(ExternalIp))]
    public override bool Installed { get; init; }
    
    public ContractId<Container> Container { get; init; } = Container ?? new ContractId<Container>("");    

    [JsonIgnore]
    public string Url => $"{Protocol.ToString().ToLower()}://{ExternalIp}:{ExternalPort}";
    
    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Port = OnlyOne(Port, contract.Port, port => port == 0),
            Container = OnlyOne(Container, contract.Container),
            ExternalPort = OnlyOne(ExternalPort, contract.ExternalPort, port => port == 0),
            ExternalIp = OnlyOne(ExternalIp, contract.ExternalIp),
        };
    }
}