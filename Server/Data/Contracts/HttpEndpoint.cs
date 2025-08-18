using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record HttpEndpoint(
    string? Name = null,
    int Port = 0,
    ContractId<Container>? Container = null,
    ContractId<Domain>? Domain = null,
    Argument<bool?>? ResultSsl = null,
    Argument<string>? ResultHost = null,
    Argument<int>? ResultPort = null,
    string? NetworkName = null, // for Traefik endpoints
    string? CloudflareZoneId = null // for Cloudflare endpoints
) : Contract(Name ?? $"{Port}{(Container != null ? $" at {Container.Name}" : "")}")
{
    [MemberNotNullWhen(true, nameof(Url))] public override bool Installed { get; init; }

    public ContractId<Container> Container { get; init; } = Container ?? new ContractId<Container>("");
    public ContractId<Domain> Domain { get; init; } = Domain ?? new ContractId<Domain>(Name ?? "");
    public Argument<bool?> ResultSsl { get; init; } = ResultSsl ?? new Argument<bool?>();
    public Argument<string> ResultHost { get; init; } = ResultHost ?? new Argument<string>();
    public Argument<int> ResultPort { get; init; } = ResultPort ?? new Argument<int>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return ResultSsl;
        yield return ResultHost;
        yield return ResultPort;
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Port = OnlyOne(Port, contract.Port, port => port == 0),
            Container = OnlyOne(Container, contract.Container),
            Domain = OnlyOne(Domain, contract.Domain),
            ResultSsl = ResultSsl.Merge(contract.ResultSsl),
            ResultHost = ResultHost.Merge(contract.ResultHost),
            ResultPort = ResultPort.Merge(contract.ResultPort),
            NetworkName = OnlyOne(NetworkName, contract.NetworkName),
            CloudflareZoneId = OnlyOne(CloudflareZoneId, contract.CloudflareZoneId)
        };
    }

    [JsonIgnore] public Uri Url => new($"http{(ResultSsl == true ? "s" : "")}://{ResultHost}:{ResultPort}");
}