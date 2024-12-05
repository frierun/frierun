using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record HttpEndpoint(
    int Port,
    string? ContainerName = null,
    string? DomainName = null
) : Contract($"{ContainerName ?? ""}:{Port}")
{
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId<Container> ContainerId => new(ContainerName);

    /// <inheritdoc />
    public override Contract With(Contract other)
    {
        if (other is not HttpEndpoint endpoint || other.Id != Id)
        {
            throw new Exception("Invalid contract");
        }

        return this with
        {
            DomainName = endpoint.DomainName ?? DomainName
        };
    }
}