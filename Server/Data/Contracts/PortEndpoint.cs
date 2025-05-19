using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record PortEndpoint(
    Protocol Protocol,
    int Port,
    string? Name = null,
    ContractId<Container>? Container = null,
    int DestinationPort = 0,
    DockerPortEndpoint? Result = null
) : Contract(Name ?? $"{(Container != null ? Container.Name + ":" : "")}{Port}/{Protocol}"), IHasResult<DockerPortEndpoint>
{
    public ContractId<Container> Container { get; init; } = Container ?? new ContractId<Container>("");    

    public override Contract With(Contract other)
    {
        if (other is not PortEndpoint endpoint || other.Id != Id)
        {
            throw new Exception("Invalid contract");
        }
        
        return this with
        {
            DestinationPort = endpoint.DestinationPort
        };
    }
}