using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class OptionalHandler : Handler<Optional>
{
    public override IEnumerable<ContractInitializeResult> Initialize(Optional contract, string prefix)
    {
        if (contract.Value is null or true)
        {
            yield return new ContractInitializeResult(
                contract with
                {
                    Handler = this,
                    Value = true
                },
                contract.Contracts
            );
        }
        
        if (contract.Value is null or false)
        {
            yield return new ContractInitializeResult(
                contract with
                {
                    Handler = this,
                    Value = false
                }
            );
        }
    }
}