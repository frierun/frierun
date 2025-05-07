using Frierun.Server.Data;
using Frierun.Server.Installers.Base;

namespace Frierun.Server.Installers.Docker;

public class LocalPathInstaller : IInstaller<Volume>
{
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
        return new LocalPath(new EmptyHandler()) { Path = contract.Path! };
    }
}