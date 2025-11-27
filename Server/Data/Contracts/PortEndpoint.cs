using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record PortEndpoint(
    Protocol Protocol,
    int Port,
    ContractId<Container>? Container = null,
    int ExternalPort = 0,
    string? ExternalIp = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(ExternalIp))]
    public override bool Installed => Id != Guid.Empty;
    
    public ContractId<Container> Container { get; init; } = Container ?? new ContractId<Container>();    

    [JsonIgnore]
    public string Url => $"{Protocol.ToString().ToLower()}://{ExternalIp}:{ExternalPort}";
    
    public override PortEndpoint Transform(IArgumentTransformer transformer)
    {
        return this with
        {
            Container = transformer.Transform(Container),
            DependsOn = DependsOn.Select(transformer.Transform).ToArray()
        };
    }
    
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