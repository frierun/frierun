using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public record InstallerInitializeResult(
    Contract Contract,
    IEnumerable<ContractId>? RequiredContracts = null,
    IEnumerable<Contract>? AdditionalContracts = null
)
{
    public IEnumerable<ContractId> RequiredContracts { get; init; } =
        RequiredContracts ?? Array.Empty<ContractId>();

    public IEnumerable<Contract> AdditionalContracts { get; init; } = AdditionalContracts ?? Array.Empty<Contract>();
}