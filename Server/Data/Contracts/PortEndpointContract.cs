using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record PortEndpointContract(
    Protocol Protocol,
    int Port,
    string? ContainerName = null,
    int DestinationPort = 0
) : Contract($"{ContainerName ?? ""}:{Port}/{Protocol}")
{
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId<ContainerContract> ContainerId => new (ContainerName);

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