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
    public override bool Installed => Id != Guid.Empty;

    public ContractRef<Network> Network { get; init; } = Network ?? new ContractRef<Network>("");
    public Argument<string> Host { get; init; } = Host ?? new Argument<string>();

    public override Redis Transform(IArgumentTransformer transformer)
    {
        return this with
        {
            Host = transformer.Transform(Host),
            DependsOn = DependsOn.Select(transformer.Transform).ToArray()
        };
    }    
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Network = MergeValue(Network, contract.Network),
            Container = MergeValue(Container, contract.Container),
            Volume = MergeValue(Volume, contract.Volume),
            Host = Host.Merge(contract.Host)
        };
    }
}