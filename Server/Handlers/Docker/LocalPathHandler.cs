using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class LocalPathHandler(Application application) : Handler<Volume>(application)
{
    public override IEnumerable<ContractInitializeResult> Initialize(Volume contract, string prefix)
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

    public override Volume Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.LocalPath != null);

        return contract;
    }
}