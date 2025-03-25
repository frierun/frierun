using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public record InstallerInitializeResult(
    Contract Contract,
    IEnumerable<Contract>? AdditionalContracts = null
)
{
    public IEnumerable<Contract> AdditionalContracts { get; init; } = AdditionalContracts ?? Array.Empty<Contract>();
}