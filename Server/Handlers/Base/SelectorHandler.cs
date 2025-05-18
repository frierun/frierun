using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Base;

public class SelectorHandler : IHandler<Selector>
{
    public IEnumerable<ContractInitializeResult> Initialize(Selector contract, string prefix)
    {
        if (contract.SelectedOption != null)
        {
            yield return new ContractInitializeResult(
                contract with { Handler = this },
                contract.Options.First(option => option.Name == contract.SelectedOption).Contracts
            );
            yield break;
        }

        foreach (var (name, contracts) in contract.Options)
        {
            yield return new ContractInitializeResult(
                contract with { SelectedOption = name, Handler = this },
                contracts
            );
        }
    }

    public Selector Install(Selector contract, ExecutionPlan plan)
    {
        if (contract.SelectedOption == null)
        {
            throw new Exception("No option selected");
        }

        return contract with
        {
            Result = new ResolvedParameter { Name = contract.Name, Value = contract.SelectedOption }
        };
    }
}