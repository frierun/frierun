using System.Diagnostics;

namespace Frierun.Server.Data;

public interface IInstaller<in TContract>: IInstaller
    where TContract : Contract
{
    public IEnumerable<ContractDependency> Dependencies(TContract contract, ExecutionPlan plan)
    {
        yield break;
    }
    
    public Contract Initialize(TContract contract, ExecutionPlan plan)
    {
        return contract;
    }
    
    public Resource? Install(TContract contract, ExecutionPlan plan)
    {
        return null;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    IEnumerable<ContractDependency> IInstaller.Dependencies(Contract contract, ExecutionPlan plan)
    {
        return Dependencies((TContract)contract, plan);
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    Contract IInstaller.Initialize(Contract contract, ExecutionPlan plan)
    {
        return Initialize((TContract)contract, plan);
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
    public IEnumerable<ContractDependency> Dependencies(Contract contract, ExecutionPlan plan);
    public Contract Initialize(Contract contract, ExecutionPlan plan);
    public Resource? Install(Contract contract, ExecutionPlan plan);
}