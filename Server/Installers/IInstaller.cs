using System.Diagnostics;
using System.Text.RegularExpressions;
using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public interface IInstaller<in TContract> : IInstaller
    where TContract : Contract
{
    public InstallerInitializeResult Initialize(TContract contract, string prefix, State state)
    {
        return new InstallerInitializeResult(contract);
    }

    public IEnumerable<ContractDependency> GetDependencies(TContract contract, ExecutionPlan plan)
    {
        yield break;
    }

    public Resource? Install(TContract contract, ExecutionPlan plan)
    {
        return null;
    }

    /// <inheritdoc />
    InstallerInitializeResult IInstaller.Initialize(Contract contract, string prefix, State state)
    {
        var result = Initialize((TContract)contract, prefix, state);
        if (contract is IHasStrings hasStrings)
        {
            var matches = new Dictionary<string, MatchCollection>();

            hasStrings.ApplyStringDecorator(
                s =>
                {
                    var matchCollection = Substitute.InsertionRegex.Matches(s);
                    if (matchCollection.Count > 0)
                    {
                        matches[s] = matchCollection;
                    }

                    return s;
                }
            );

            if (matches.Count > 0)
            {
                result = result with
                {
                    AdditionalContracts = result.AdditionalContracts.Append(new Substitute(contract.Id, matches))
                };
            }
        }

        return result with
        {
            Contract = result.Contract with
            {
                Installer = GetType().Name
            }
        };
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<ContractDependency> IInstaller.GetDependencies(Contract contract, ExecutionPlan plan)
    {
        return GetDependencies((TContract)contract, plan);
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    Resource? IInstaller.Install(Contract contract, ExecutionPlan plan)
    {
        return Install((TContract)contract, plan);
    }
}

public interface IInstaller
{
    /// <summary>
    /// Initializes contract
    /// </summary>
    public InstallerInitializeResult Initialize(Contract contract, string prefix, State state);

    /// <summary>
    /// Returns all contract edges derived from the contract
    /// </summary>
    public IEnumerable<ContractDependency> GetDependencies(Contract contract, ExecutionPlan plan);

    /// <summary>
    /// Installs the contract
    /// </summary>
    public Resource? Install(Contract contract, ExecutionPlan plan);
}