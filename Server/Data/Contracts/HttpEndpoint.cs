using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record HttpEndpoint(
    string? Name = null,
    int Port = 0,
    string? ContainerName = null
) : Contract(Name ?? $"{Port}{(ContainerName != null ? $" at {ContainerName}" : "")}")
{
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId<Container> ContainerId => new(ContainerName);
    [JsonIgnore] public ContractId<Domain> DomainId => new(Name);

    /// <inheritdoc />
    public override Contract With(Contract other)
    {
        if (other is not HttpEndpoint endpoint || other.Id != Id)
        {
            throw new Exception("Invalid contract");
        }

        return this with
        {
            Installer = endpoint.Installer ?? Installer,
        };
    }
}