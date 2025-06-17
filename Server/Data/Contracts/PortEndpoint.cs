using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

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

    public override Contract With(Contract other)
    {
        if (other is not PortEndpoint endpoint || other.Id != Id)
        {
            throw new Exception("Invalid contract");
        }
        
        return this with
        {
            ExternalPort = ExternalPort == 0 ? endpoint.ExternalPort : ExternalPort,
        };
    }
    
    [JsonIgnore]
    public string Url => $"{Protocol.ToString().ToLower()}://{ExternalIp}:{ExternalPort}";
}