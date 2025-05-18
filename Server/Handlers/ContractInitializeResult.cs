using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public record ContractInitializeResult(
    Contract Contract,
    IEnumerable<Contract>? AdditionalContracts = null
)
{
    public IEnumerable<Contract> AdditionalContracts { get; init; } = AdditionalContracts ?? Array.Empty<Contract>();
}