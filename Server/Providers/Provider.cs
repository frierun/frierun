using System.Diagnostics;
using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public abstract class
    Provider<TResource, TDefinition> : Provider<TResource, TDefinition, ExecutionPlan<TResource, TDefinition>>
    where TResource : Resource
    where TDefinition : ResourceDefinition<TResource>
{
    public override ExecutionPlan CreatePlan(State state, ResourceDefinition definition, ExecutionPlan? parent)
    {
        var plan = new ExecutionPlan<TResource, TDefinition>(state, (TDefinition)definition, this, parent);
        FillParameters(plan);
        return plan;
    }
}

public abstract class Provider<TResource, TDefinition, TExecutionPlan> : Provider
    where TResource : Resource
    where TDefinition : ResourceDefinition<TResource>
    where TExecutionPlan : ExecutionPlan<TResource, TDefinition>
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
    
    protected abstract void Uninstall(TResource resource);

    /// <inheritdoc />
    [DebuggerStepThrough]
    public override void Uninstall(Resource resource)
    {
        Uninstall((TResource)resource);
    }

    private string GetFullName(TExecutionPlan plan)
    {
        if (!plan.Parameters.TryGetValue("name", out var name))
        {
            name = plan.Definition.Name ?? "";
        }

        if (plan.Parent == null)
        {
            return name;
        }
        
        var parentName = plan.Parent.GetFullName();
        if (name == "")
        {
            return parentName;
        }
        
        return $"{parentName}-{name}";
    }

    /// <inheritdoc />
    public override string GetFullName(ExecutionPlan plan)
    {
        return GetFullName((TExecutionPlan)plan);
    }
}

public abstract class Provider
{
    public abstract ExecutionPlan CreatePlan(State state, ResourceDefinition definition, ExecutionPlan? parent);
    public abstract bool Validate(ExecutionPlan plan);
    public abstract Resource Install(ExecutionPlan plan);
    public abstract void Uninstall(Resource resource);

    public abstract string GetFullName(ExecutionPlan plan);
}