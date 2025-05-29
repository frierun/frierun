using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace Frierun.Server.Data;

public record HttpEndpoint(
    string? Name = null,
    int Port = 0,
    ContractId<Container>? Container = null,
    ContractId<Domain>? Domain = null,
    Uri? Url = null,
    string? NetworkName = null // for Traefik endpoints
) : Contract(Name ?? $"{Port}{(Container != null ? $" at {Container.Name}" : "")}")
{
    [MemberNotNullWhen(true, nameof(Url))]
    public override bool Installed { get; init; }
    
    public ContractId<Container> Container { get; init; } = Container ?? new ContractId<Container>("");
    public ContractId<Domain> Domain { get; init; } = Domain ?? new ContractId<Domain>(Name ?? "");

    public override Contract With(Contract other)
    {
        if (other is not HttpEndpoint endpoint || other.Id != Id)
        {
            throw new Exception("Invalid contract");
        }

        return this with
        {
            Handler = endpoint.Handler,
        };
    }
    
    [JsonIgnore]
    public string? Host => Url?.Host;
}