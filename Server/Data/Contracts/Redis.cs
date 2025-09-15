using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Redis(
    ContractId<Network>? Network = null,
    ContractId<Container>? Container = null,
    ContractId<Volume>? Volume = null,
    Argument<string>? Host = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(Container), nameof(Volume))]
    public override bool Installed { get; init; }

    public ContractId<Network> Network { get; init; } = Network ?? new ContractId<Network>("");
    public Argument<string> Host { get; init; } = Host ?? new Argument<string>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return Host;
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Network = OnlyOne(Network, contract.Network),
            Container = OnlyOne(Container, contract.Container),
            Volume = OnlyOne(Volume, contract.Volume),
            Host = Host.Merge(contract.Host)
        };
    }
}