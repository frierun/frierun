using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record PortEndpointContract(
    PortType PortType,
    int Port,
    string? ContainerName = null,
    int DestinationPort = 0
) : Contract<PortEndpoint>($"{ContainerName ?? ""}:{Port}/{PortType}")
{
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId ContainerId => new (typeof(Container), ContainerName);

    /// <inheritdoc />
    public override Contract With(Contract other)
    {
        if (other.Id != Id)
        {
            throw new Exception("Invalid contract id");
        }
        
        if (other is not PortEndpointContract endpoint)
        {
            throw new Exception("Invalid contract type");
        }
        
        return this with
        {
            DestinationPort = endpoint.DestinationPort
        };
    }
}