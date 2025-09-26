using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Redis(
    ContractRef<Network>? Network = null,
    ContractRef<Container>? Container = null,
    ContractRef<Volume>? Volume = null,
    Argument<string>? Host = null
) : Contract
{
    [MemberNotNullWhen(true, nameof(Container), nameof(Volume))]
    public override bool Installed => Id != null;

    public ContractRef<Network> Network { get; init; } = Network ?? new ContractRef<Network>("");
    public Argument<string> Host { get; init; } = Host ?? new Argument<string>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return Host;
        foreach (var argument in base.GetArguments())
        {
            yield return argument;
        }
    }

    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Network = OnlyOne(Network, contract.Network),
            Container = OnlyOne(Container, contract.Container),
            Volume = OnlyOne(Volume, contract.Volume),
            Host = Host.Merge(contract.Host)
        };
    }
}