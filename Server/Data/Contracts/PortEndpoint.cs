using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record PortEndpoint(
    Protocol Protocol,
    int Port,
    ContractRef<Container>? Container = null,
    int ExternalPort = 0,
    string? ExternalIp = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(ExternalIp))]
    public override bool Installed => Id != Guid.Empty;
    
    public ContractRef<Container> Container { get; init; } = Container ?? new ContractRef<Container>("");    

    [JsonIgnore]
    public string Url => $"{Protocol.ToString().ToLower()}://{ExternalIp}:{ExternalPort}";
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Port = MergeValue(Port, contract.Port, port => port == 0),
            Container = MergeValue(Container, contract.Container),
            ExternalPort = MergeValue(ExternalPort, contract.ExternalPort, port => port == 0),
            ExternalIp = MergeValue(ExternalIp, contract.ExternalIp),
        };
    }
}