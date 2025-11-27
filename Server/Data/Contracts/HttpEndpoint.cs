using System.Text.Json.Serialization;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record HttpEndpoint(
    int Port = 0,
    ContractId<Container>? Container = null,
    Argument<bool?>? ResultSsl = null,
    Argument<string>? ResultHost = null,
    Argument<int>? ResultPort = null,
    string? TraefikRouterName = null, // for Traefik endpoints
    string? NetworkName = null, // for Traefik endpoints
    string? CloudflareZoneId = null // for Cloudflare endpoints
) : Contract
{
    public ContractId<Container> Container { get; init; } = Container ?? new ContractId<Container>();
    public Argument<bool?> ResultSsl { get; init; } = ResultSsl ?? new Argument<bool?>();
    public Argument<string> ResultHost { get; init; } = ResultHost ?? new Argument<string>();
    public Argument<int> ResultPort { get; init; } = ResultPort ?? new Argument<int>();

    public override HttpEndpoint Transform(IArgumentTransformer transformer)
    {
        return this with
        {
            Container = transformer.Transform(Container),
            ResultSsl = transformer.Transform(ResultSsl),
            ResultHost = transformer.Transform(ResultHost),
            ResultPort = transformer.Transform(ResultPort),
            DependsOn = DependsOn.Select(transformer.Transform).ToArray()
        };
    }
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Port = MergeValue(Port, contract.Port, port => port == 0),
            Container = MergeValue(Container, contract.Container),
            ResultSsl = MergeValue(ResultSsl, contract.ResultSsl),
            ResultHost = MergeValue(ResultHost, contract.ResultHost),
            ResultPort = MergeValue(ResultPort, contract.ResultPort),
            TraefikRouterName = MergeValue(TraefikRouterName, contract.TraefikRouterName),
            NetworkName = MergeValue(NetworkName, contract.NetworkName),
            CloudflareZoneId = MergeValue(CloudflareZoneId, contract.CloudflareZoneId)
        };
    }

    [JsonIgnore] public UriArgument Url => new(ResultSsl, ResultHost, ResultPort);
}