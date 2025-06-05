using System.Diagnostics;
using System.Text.RegularExpressions;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class Handler<TContract>(Application? application = null) : IHandler
    where TContract : Contract
{
    public Application? Application => application;

    public virtual IEnumerable<ContractInitializeResult> Initialize(TContract contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                Handler = this
            }
        );
    }

    public virtual TContract Install(TContract contract, ExecutionPlan plan)
    {
        return contract;
    }

    public virtual void Uninstall(TContract contract)
    {
        // do nothing
    }

    IEnumerable<ContractInitializeResult> IHandler.Initialize(Contract contract, string prefix)
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