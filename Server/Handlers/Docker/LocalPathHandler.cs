using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class LocalPathHandler(Application application) : IHandler<Volume>
{
    public Application Application => application;

    public IEnumerable<ContractInitializeResult> Initialize(Volume contract, string prefix)
    {
        if (contract.VolumeName != null)
        {
            yield break;
        }
        
        if (contract.LocalPath != null)
        {
            yield return new ContractInitializeResult(contract with { Handler = this });
        }
    }

    public Volume Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.LocalPath != null);

        return contract;
    }
}