using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record HttpEndpoint(
    int Port = 0,
    ContractRef<Container>? Container = null,
    Argument<bool?>? ResultSsl = null,
    Argument<string>? ResultHost = null,
    Argument<int>? ResultPort = null,
    string? TraefikRouterName = null, // for Traefik endpoints
    string? NetworkName = null, // for Traefik endpoints
    string? CloudflareZoneId = null // for Cloudflare endpoints
) : Contract
{
    public ContractRef<Container> Container { get; init; } = Container ?? new ContractRef<Container>("");
    public Argument<bool?> ResultSsl { get; init; } = ResultSsl ?? new Argument<bool?>();
    public Argument<string> ResultHost { get; init; } = ResultHost ?? new Argument<string>();
    public Argument<int> ResultPort { get; init; } = ResultPort ?? new Argument<int>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return ResultSsl;
        yield return ResultHost;
        yield return ResultPort;
        foreach (var argument in base.GetArguments())
        {
            yield return argument;
        }
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Port = OnlyOne(Port, contract.Port, port => port == 0),
            Container = OnlyOne(Container, contract.Container),
            ResultSsl = ResultSsl.Merge(contract.ResultSsl),
            ResultHost = ResultHost.Merge(contract.ResultHost),
            ResultPort = ResultPort.Merge(contract.ResultPort),
            TraefikRouterName = OnlyOne(TraefikRouterName, contract.TraefikRouterName),
            NetworkName = OnlyOne(NetworkName, contract.NetworkName),
            CloudflareZoneId = OnlyOne(CloudflareZoneId, contract.CloudflareZoneId)
        };
    }

    [JsonIgnore] public Uri Url => new($"http{(ResultSsl == true ? "s" : "")}://{ResultHost}:{ResultPort}");
}