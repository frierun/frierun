﻿using Frierun.Server.Data;

namespace Frierun.Server.Installers.Base;

public class SelectorInstaller : IInstaller<Selector>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<Selector>.Initialize(
        Selector contract,
        string prefix,
        State state
    )
    {
        if (contract.SelectedOption != null)
        {
            yield return new InstallerInitializeResult(
                contract,
                null,
                contract.Options.First(option => option.Name == contract.SelectedOption).Contracts
            );
            yield break;
        }

        foreach (var (name, contracts) in contract.Options)
        {
            yield return new InstallerInitializeResult(
                contract with { SelectedOption = name },
                null,
                contracts
            );
        }
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<Selector>.GetDependencies(Selector selector, ExecutionPlan plan)
    {
        var package = plan.Contracts.Values.OfType<Package>().First();
        return selector
            .Options
            .First(option => option.Name == selector.SelectedOption)
            .Contracts
            .Select(contract => new ContractDependency(contract.Id, package.Id));
    }
}