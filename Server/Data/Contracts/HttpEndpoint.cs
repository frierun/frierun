using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record HttpEndpoint(
    string? Name = null,
    int Port = 0,
    ContractId<Container>? Container = null,
    ContractId<Domain>? Domain = null,
    GenericHttpEndpoint? Result = null
) : Contract(Name ?? $"{Port}{(Container != null ? $" at {Container.Name}" : "")}"), IHasResult<GenericHttpEndpoint>
{
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
}