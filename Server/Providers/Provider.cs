using System.Diagnostics;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public abstract class Provider<TResource, TDefinition> : Provider 
    where TResource : Resource
    where TDefinition : ResourceDefinition<TResource>
{
    public override ExecutionPlan CreatePlan(State state, ResourceDefinition definition, ExecutionPlan? parent = null)
    {
        var plan = new ExecutionPlan<TResource, TDefinition>(state, (TDefinition)definition, this, parent);
        FillParameters(plan);
        return plan;
    }
    
    protected abstract void FillParameters(ExecutionPlan<TResource, TDefinition> plan);
    
    protected abstract bool Validate(ExecutionPlan<TResource, TDefinition> plan);

    /// <inheritdoc />
    [DebuggerStepThrough]
    public override bool Validate(ExecutionPlan plan)
    {
        return Validate((ExecutionPlan<TResource, TDefinition>)plan);
    }

    protected abstract TResource Install(ExecutionPlan<TResource, TDefinition> plan);

    /// <inheritdoc />
    [DebuggerStepThrough]
    public override Resource Install(ExecutionPlan plan)
    {
        return Install((ExecutionPlan<TResource, TDefinition>)plan);
    }
}

public abstract class Provider
{
    public abstract ExecutionPlan CreatePlan(State state, ResourceDefinition definition, ExecutionPlan? parent = null);
    public abstract bool Validate(ExecutionPlan plan);
    public abstract Resource Install(ExecutionPlan plan);
}