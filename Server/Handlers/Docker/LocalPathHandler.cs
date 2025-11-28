using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Docker;

public class LocalPathHandler(State state, Application application) : Handler<Volume>(state, application)
{
    public override IEnumerable<ContractList> Initialize(Volume contract, ApplicationContext context)
    {
        if (contract.Installed)
        {
            yield break;
        }
        
        if (contract.VolumeName != null)
        {
            yield break;
        }

        yield return new ContractList
            {
                [context] = contract with
                {
                    LocalPath = contract.LocalPath ?? $"/data/{context.Prefix}/{context.Name}",
                    Handler = this
                }
            };
    }

    public override Volume Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.LocalPath != null);

        return contract;
    }
}