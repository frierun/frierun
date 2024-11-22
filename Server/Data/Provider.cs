using System.Diagnostics;

namespace Frierun.Server.Data;

public abstract class Provider<TResource, TContract> : Provider
    where TResource : Resource
    where TContract : Contract<TResource>
{
    /// <inheritdoc />
    [DebuggerStepThrough]
    public sealed override IEnumerable<ContractDependency> Dependencies(Contract contract, ExecutionPlan plan)
    {
        return Dependencies((TContract)contract, plan);
    }

    protected virtual IEnumerable<ContractDependency> Dependencies(TContract contract, ExecutionPlan plan)
    {
        yield break;
    }

    /// <inheritdoc />
    [DebuggerStepThrough]
    public sealed override Contract Initialize(Contract contract, ExecutionPlan plan)
    {
        return Initialize((TContract)contract, plan);
    }

    protected virtual TContract Initialize(TContract contract, ExecutionPlan plan)
    {
        return contract;
    }
    
    /// <inheritdoc />
    [DebuggerStepThrough]
    public sealed override Resource Install(Contract contract, ExecutionPlan plan)
    {
        return Install((TContract)contract, plan);
    }

    protected abstract TResource Install(TContract contract, ExecutionPlan plan);

    /// <inheritdoc />
    [DebuggerStepThrough]
    public sealed override void Uninstall(Resource resource)
    {
        Uninstall((TResource)resource);
    }

    protected virtual void Uninstall(TResource resource)
    {
        
    }
}

public abstract class Provider
{
    public abstract IEnumerable<ContractDependency> Dependencies(Contract contract, ExecutionPlan plan);
    public abstract Contract Initialize(Contract contract, ExecutionPlan plan);
    public abstract Resource Install(Contract contract, ExecutionPlan plan);
    public abstract void Uninstall(Resource resource);
}