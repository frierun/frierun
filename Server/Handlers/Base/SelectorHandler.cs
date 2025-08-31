using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class SelectorHandler : Handler<Selector>
{
    public override IEnumerable<ContractList> Initialize(Selector contract, ApplicationContext context)
    {
        if (contract.Value != null)
        {
            yield return
            [
                contract with
                {
                    Handler = this
                },
                ..contract.Options.First(option => option.Name == contract.Value).Contracts ?? []
            ];
            yield break;
        }

        foreach (var (name, contracts) in contract.Options)
        {
            yield return
            [
                contract with
                {
                    Value = name,
                    Handler = this
                },
                ..contracts ?? []
            ];
        }
    }
}