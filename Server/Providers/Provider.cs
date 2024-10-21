using System.Diagnostics;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public abstract class
    Provider<TResource, TDefinition> : Provider<TResource, TDefinition, ExecutionPlan<TDefinition>>
    where TResource : Resource
    where TDefinition : ResourceDefinition<TResource>
{
    public override ExecutionPlan CreatePlan(State state, ResourceDefinition definition, ExecutionPlan? parent)
    {
        var plan = new ExecutionPlan<TDefinition>(state, (TDefinition)definition, this, parent);
        FillParameters(plan);
        return plan;
    }
}

public abstract class Provider<TResource, TDefinition, TExecutionPlan> : Provider
    where TResource : Resource
    where TDefinition : ResourceDefinition<TResource>
    where TExecutionPlan : ExecutionPlan<TDefinition>
{
    protected abstract void FillParameters(TExecutionPlan plan);

    protected abstract bool Validate(TExecutionPlan plan);

    /// <inheritdoc />
    [DebuggerStepThrough]
    public override bool Validate(ExecutionPlan plan)
    {
        return Validate((TExecutionPlan)plan);
    }

    protected abstract TResource Install(TExecutionPlan plan);

    /// <inheritdoc />
    [DebuggerStepThrough]
    public override Resource Install(ExecutionPlan plan)
    {
        return Install((TExecutionPlan)plan);
    }
}

public abstract class Provider
{
    public abstract ExecutionPlan CreatePlan(State state, ResourceDefinition definition, ExecutionPlan? parent);
    public abstract bool Validate(ExecutionPlan plan);
    public abstract Resource Install(ExecutionPlan plan);
}