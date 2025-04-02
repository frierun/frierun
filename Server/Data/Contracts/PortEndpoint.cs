using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record PortEndpoint(
    Protocol Protocol,
    int Port,
    string? Name = null,
    string? ContainerName = null,
    int DestinationPort = 0
) : Contract(Name ?? $"{(ContainerName != null ? ContainerName + ":" : "")}{Port}/{Protocol}")
{
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId<Container> ContainerId => new (ContainerName);

    /// <inheritdoc />
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