using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class LocalPathHandler(Application application) : Handler<Volume>(application)
{
    public override IEnumerable<ContractInitializeResult> Initialize(Volume contract, ApplicationContext context)
    {
        if (contract.VolumeName != null)
        {
            yield break;
        }
        
        yield return new ContractInitializeResult(contract with
        {
            LocalPath = contract.LocalPath ?? $"/data/{context.Prefix}/{context.Name}",
            Handler = this
        });
    }

    public override Volume Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.LocalPath != null);

        return contract;
    }
}