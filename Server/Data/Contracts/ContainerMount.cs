namespace Frierun.Server.Data;

public record ContainerMount(
    ContractRef<Volume>? Volume = null,
    bool ReadOnly = false
)
{
    public ContractRef<Volume> Volume { get; init; } = Volume ?? new ContractRef<Volume>();
}