namespace Frierun.Server.Data;

public record ContainerMount(
    ContractId<Volume>? Volume = null,
    bool ReadOnly = false
)
{
    public ContractId<Volume> Volume { get; init; } = Volume ?? new ContractId<Volume>("");
}