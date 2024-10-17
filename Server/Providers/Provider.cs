using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Providers;

public abstract class Provider<TResource, TDefinition> : Provider 
    where TResource : Resource
    where TDefinition : ResourceDefinition<TResource>
{
    protected abstract IDictionary<string, string> GetParameters(ExecutionPlan plan, TDefinition definition);

    /// <inheritdoc />
    public override IDictionary<string, string> GetParameters(ExecutionPlan plan, ResourceDefinition definition)
    {
        return GetParameters(plan, (TDefinition)definition);
    }

    protected abstract TResource Create(ExecutionPlan plan, IDictionary<string, string> parameters,
        TDefinition definition);

    /// <inheritdoc />
    public override object Install(ExecutionPlan plan, IDictionary<string, string> parameters,
        ResourceDefinition definition)
    {
        return Create(plan, parameters, (TDefinition)definition);
    }
}

public abstract class Provider
{
    public abstract IDictionary<string, string> GetParameters(ExecutionPlan plan, ResourceDefinition definition);
    public abstract bool Validate(ExecutionPlan plan, IDictionary<string, string> parameters);
    public abstract object Install(ExecutionPlan plan, IDictionary<string, string> parameters,
        ResourceDefinition definition);
}