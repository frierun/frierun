using System.Diagnostics;
using Frierun.Server.Data;
using Frierun.Server.Installers.Base;

namespace Frierun.Server.Installers.Docker;

public class LocalPathInstaller(Application application) : IInstaller<Volume>
{
    public Application Application => application;

    IEnumerable<InstallerInitializeResult> IInstaller<Volume>.Initialize(Volume contract, string prefix)
    {
        if (contract.Path != null)
        {
            yield return new InstallerInitializeResult(contract);
        }
    }

    Volume IInstaller<Volume>.Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.Path != null, "Path should not be null");
        
        return contract with
        {
            Result = new LocalPath(new EmptyHandler()) { Path = contract.Path }
        };
    }
}