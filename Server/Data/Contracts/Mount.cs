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
}