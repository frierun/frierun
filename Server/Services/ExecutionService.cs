using Frierun.Server.Models;
using Frierun.Server.Resources;

namespace Frierun.Server.Services;

public class ExecutionService(
    ProviderRegistry providerRegistry,
    State state
)
{
    public ExecutionPlan Create(ResourceDefinition definition, ExecutionPlan? parent = null)
    {
        var provider = providerRegistry.Get(definition.ResourceType);
        if (provider == null)
        {
            throw new Exception($"Can't find provider for resource {typeof(Application)}");
        }
        var executionPlan = provider.CreatePlan(state, definition, parent);

        foreach (var childResource in definition.Children)
        {
            executionPlan.Children.Add(Create(childResource, executionPlan));
        }
        
        return executionPlan;
    }
    
    /// <summary>
    /// Validates the execution plan and all its children
    /// </summary>
    public bool Validate(ExecutionPlan executionPlan)
    {
        if (!executionPlan.Validate())
        {
            return false;
        }

        return executionPlan.Children.All(Validate);
    }
}