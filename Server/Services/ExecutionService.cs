using Frierun.Server.Models;
using Frierun.Server.Providers;
using Frierun.Server.Resources;

namespace Frierun.Server.Services;

public class ExecutionService(
    ProviderRegistry providerRegistry,
    State state
)
{
    public ExecutionPlan Create(ResourceDefinition definition, ExecutionPlan? parent = null)
    {
        var providers = providerRegistry.Get(definition.ResourceType);
        if (providers.Count == 0)
        {
            throw new Exception($"Can't find provider for resource {typeof(Application)}");
        }
        
        if (providers.Count == 1)
        {
            return Create(definition, parent, providers[0]);
        }

        var provider = new SelectorProvider();
        var executionPlan = provider.CreatePlan(state, definition, parent);

        foreach (var childProvider in providers)
        {
            executionPlan.Children.Add(Create(definition, parent, childProvider));
        }
        
        return executionPlan;
    }
    
    private ExecutionPlan Create(ResourceDefinition definition, ExecutionPlan? parent, Provider provider)
    {
        var executionPlan = provider.CreatePlan(state, definition, parent);

        foreach (var childDefinition in definition.Children)
        {
            executionPlan.Children.Add(Create(childDefinition, executionPlan));
        }
        
        return executionPlan;
    }    
}