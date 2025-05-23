using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record Redis(
    string? Name = null,
    ContractId<Network>? Network = null,
    ContractId<Container>? Container = null,
    ContractId<Volume>? Volume = null,
    string? Host = null
) : Contract(Name ?? "")
{
    [MemberNotNullWhen(true, nameof(Host), nameof(Container), nameof(Volume))]
    public override bool Installed { get; init; }

    public ContractId<Network> Network { get; init; } = Network ?? new ContractId<Network>("");
}