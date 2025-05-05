using Frierun.Server.Data;

namespace Frierun.Server.Installers.Docker;

public class LocalPathInstaller : IInstaller<Volume>, IUninstaller<LocalPath>
{
    /// <inheritdoc />
    public Application? Application => null;

    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Volume>.Initialize(Volume contract, string prefix)
    {
        if (contract.Path != null)
        {
            yield return new InstallerInitializeResult(contract);
        }
    }

    /// <inheritdoc />
    Resource IInstaller<Volume>.Install(Volume contract, ExecutionPlan plan)
    {
        return new LocalPath { Path = contract.Path! };
    }
}