using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record HttpEndpointContract(
    int Port,
    string? ContainerName = null,
    string? DomainName = null
) : Contract($"{ContainerName ?? ""}:{Port}")
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
        
        if (other is not HttpEndpointContract endpoint)
        {
            throw new Exception("Invalid contract type");
        }
        
        return this with
        {
            DomainName = endpoint.DomainName ?? DomainName
        };
    }
}