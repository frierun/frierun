using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Udocker;

public class LocalPathHandler(Application application) : Handler<Volume>(application)
{
    private const string HomePath = "/data/data/com.termux/files/home";

    
    public override IEnumerable<ContractInitializeResult> Initialize(Volume contract, string prefix)
    {
        if (contract.VolumeName != null)
        {
            yield break;
        }
        
        yield return new ContractInitializeResult(contract with
        {
            LocalPath = contract.LocalPath ?? $"{HomePath}/frierun/{prefix}/{contract.Name}",
            Handler = this
        });
    }

    public override Volume Install(Volume contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.LocalPath != null);

        return contract;
    }
}