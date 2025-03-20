using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class ParameterInstaller : IInstaller<Parameter>, IUninstaller<ResolvedParameter>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Parameter>.Initialize(Parameter contract, string prefix)
    {
        var value = contract.Value ?? contract.DefaultValue;

        yield return new InstallerInitializeResult(
            contract with { Value = value }
        );
    }

    /// <inheritdoc />
    Resource IInstaller<Parameter>.Install(Parameter contract, ExecutionPlan plan)
    {
        return new ResolvedParameter(contract.Name, contract.Value);
    }
}