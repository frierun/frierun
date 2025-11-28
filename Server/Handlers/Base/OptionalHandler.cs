using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class OptionalHandler(State state) : Handler<Optional>(state)
{
    public override IEnumerable<ContractList> Initialize(Optional contract, ApplicationContext context)
    {
        if (contract.Value is null or true)
        {
            yield return new ContractList(contract.Contracts)
            {
                [context] = contract with
                {
                    Handler = this,
                    Value = true
                },
            };
        }

        if (contract.Value is null or false)
        {
            yield return new ContractList
            {
                [context] = contract with
                {
                    Handler = this,
                    Value = false
                }
            };
        }
    }
}