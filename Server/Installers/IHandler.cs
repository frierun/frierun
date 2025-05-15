using System.Diagnostics;
using System.Text.RegularExpressions;
using Frierun.Server.Data;

namespace Frierun.Server.Installers;

public interface IHandler<TContract>: IHandler
    where TContract : Contract
{
    public IEnumerable<InstallerInitializeResult> Initialize(TContract contract, string prefix)
    {
        yield return new InstallerInitializeResult(contract with
        {
            Handler = this
        });
    }

    public TContract Install(TContract contract, ExecutionPlan plan)
    {
        return contract;
    }

    void Uninstall(TContract resource)
    {
        // do nothing
    }

    IEnumerable<InstallerInitializeResult> IHandler.Initialize(Contract contract, string prefix)
    {
        foreach (var result in Initialize((TContract)contract, prefix))
        {
            Debug.Assert(result.Contract.Handler != null);
            
            if (result.Contract is IHasStrings hasStrings)
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
                    yield return result with
                    {
                        AdditionalContracts = result.AdditionalContracts.Append(new Substitute(contract, matches))
                    };
                    continue;
                }
            }
            
            yield return result;
        }
    }

    [DebuggerStepThrough]
    Contract IHandler.Install(Contract contract, ExecutionPlan plan)
    {
        return Install((TContract)contract, plan);
    }
    
    [DebuggerStepThrough]
    void IHandler.Uninstall(Contract contract)
    {
        Uninstall((TContract)contract);
    }
}

public interface IHandler
{
    public Application? Application => null;
    
    /// <summary>
    /// Returns all possible ways to initializes contract
    /// </summary>
    public IEnumerable<InstallerInitializeResult> Initialize(Contract contract, string prefix);

    /// <summary>
    /// Installs the contract
    /// </summary>
    public Contract Install(Contract contract, ExecutionPlan plan);
    
    /// <summary>
    /// Uninstalls the contract
    /// </summary>
    void Uninstall(Contract contract);
}