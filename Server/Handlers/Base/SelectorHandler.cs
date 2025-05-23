using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class SelectorHandler : IHandler<Selector>
{
    public IEnumerable<ContractInitializeResult> Initialize(Selector contract, string prefix)
    {
        if (contract.Value != null)
        {
            yield return new ContractInitializeResult(
                contract with
                {
                    Handler = this
                },
                contract.Options.First(option => option.Name == contract.Value).Contracts
            );
            yield break;
        }

        foreach (var (name, contracts) in contract.Options)
        {
            yield return new ContractInitializeResult(
                contract with
                {
                    Value = name,
                    Handler = this
                },
                contracts
            );
        }
    }
}