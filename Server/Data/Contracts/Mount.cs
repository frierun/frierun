using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Mount(
    string Path,
    ContractId<Volume>? Volume = null,
    ContractId<Container>? Container = null,
    bool ReadOnly = false
) : Contract($"{Volume?.Name}{(Container == null ? "" : $" in {Container.Name}: ")}")
{
    public ContractId<Volume> Volume { get; init; } = Volume ?? new ContractId<Volume>("");
    public ContractId<Container> Container { get; init; } = Container ?? new ContractId<Container>("");

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, contract) with
        {
            Path = OnlyOne(Path, contract.Path),
            Volume = OnlyOne(Volume, contract.Volume),
            Container = OnlyOne(Container, contract.Container),
            ReadOnly = ReadOnly || contract.ReadOnly
        };
    }
}