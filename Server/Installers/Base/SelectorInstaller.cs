using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class SelectorInstaller : IInstaller<Selector>
{
    public Application? Application => null;

    IEnumerable<InstallerInitializeResult> IInstaller<Selector>.Initialize(Selector contract, string prefix)
    {
        if (contract.SelectedOption != null)
        {
            yield return new InstallerInitializeResult(
                contract,
                contract.Options.First(option => option.Name == contract.SelectedOption).Contracts
            );
            yield break;
        }

        foreach (var (name, contracts) in contract.Options)
        {
            yield return new InstallerInitializeResult(
                contract with { SelectedOption = name },
                contracts
            );
        }
    }

    Selector IInstaller<Selector>.Install(Selector contract, ExecutionPlan plan)
    {
        if (contract.SelectedOption == null)
        {
            throw new Exception("No option selected");
        }

        return contract with
        {
            Result = new ResolvedParameter(new EmptyHandler()) { Name = contract.Name, Value = contract.SelectedOption }
        };
    }
}