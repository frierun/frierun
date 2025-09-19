using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Udocker;

public class LocalPathHandler(State state, Application application) : Handler<Volume>(state, application)
{
    private const string HomePath = "/data/data/com.termux/files/home";


    public override IEnumerable<ContractList> Initialize(Volume contract, ApplicationContext context)
    {
        if (contract.VolumeName != null)
        {
            yield break;
        }

        yield return new ContractList
        {
            [context] = contract with
            {
                LocalPath = contract.LocalPath ?? $"{HomePath}/frierun/{context.Prefix}/{context.Name}",
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