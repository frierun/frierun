using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class OptionalHandler : Handler<Optional>
{
    public override IEnumerable<ContractList> Initialize(Optional contract, ApplicationContext context)
    {
        if (contract.Value is null or true)
        {
            yield return
            [
                contract with
                {
                    Handler = this,
                    Value = true
                },
                ..contract.Contracts
            ];
        }

        if (contract.Value is null or false)
        {
            yield return
            [
                contract with
                {
                    Handler = this,
                    Value = false
                }
            ];
        }
    }
}