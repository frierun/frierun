using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers;

public class Handler<TContract>(Application? application = null) : IHandler
    where TContract : Contract
{
    public required State State { protected get; init; }
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
        return Initialize((TContract)contract, prefix);
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

    /// <summary>
    /// Finds a unique name for a contract property
    /// </summary>
    protected string FindUniqueName(
        string baseName,
        Func<TContract, string?> predicate,
        string suffix = "",
        IReadOnlyList<string>? forbidden = null
    )
    {
        var count = 1;
        var name = $"{baseName}{suffix}";
        while (forbidden?.Contains(name) == true || State.Contracts.OfType<TContract>().Any(c => predicate(c) == name))
        {
            count++;
            name = $"{baseName}{count}{suffix}";
        }

        return name;
    }
}