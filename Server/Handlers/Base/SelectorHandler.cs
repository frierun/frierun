using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class SelectorHandler(State state) : Handler<Selector>(state)
{
    public override IEnumerable<ContractList> Initialize(Selector contract, ApplicationContext context)
    {
        foreach (var (name, contracts) in contract.Options)
        {
            if (contract.Value != null && contract.Value != name)
            {
                continue;
            }

            yield return new ContractList(contracts ?? [])
            {
                [context] = contract with
                {
                    Value = name,
                    Handler = this
                }
            };
        }
    }
}