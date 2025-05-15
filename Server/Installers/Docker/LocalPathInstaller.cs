using System.Diagnostics;
using Frierun.Server.Data;
using Frierun.Server.Installers.Base;

namespace Frierun.Server.Installers.Docker;

public class LocalPathInstaller(Application application) : IHandler<Volume>
{
    public Application Application => application;

    public IEnumerable<InstallerInitializeResult> Initialize(Volume contract, string prefix)
    {
        if (contract.Path != null)
        {
            yield return new InstallerInitializeResult(contract with { Handler = this });
        }
    }

    public Volume Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.Path != null, "Path should not be null");

        return contract with
        {
            Result = new LocalPath { Path = contract.Path }
        };
    }
}