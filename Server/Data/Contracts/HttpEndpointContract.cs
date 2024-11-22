using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record HttpEndpointContract(
    int Port,
    string? ContainerName = null,
    string? DomainName = null
) : Contract<HttpEndpoint>($"{ContainerName ?? ""}:{Port}")
{
    public string ContainerName { get; init; } = ContainerName ?? "";
    [JsonIgnore] public ContractId ContainerId => new (typeof(Container), ContainerName);
}