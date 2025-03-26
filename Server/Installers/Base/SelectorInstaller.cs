using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class SelectorInstaller : IInstaller<Selector>
{
    /// <inheritdoc />
    public Application? Application => null;
    
    /// <inheritdoc />
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
}