using Frierun.Server.Models;

namespace Frierun.Server.Services;

public class ExecutionService(
    ILogger<ExecutionService> logger,
    ProviderRegistry providerRegistry,
    State state
)
{
    public ExecutionPlan? Create(Package package)
    {
        var name = package.Name;
        int count = 1;
        while (state.Applications.Any(application => application.Name == name))
        {
            count++;
            name = $"{package.Name}{count}";
        }
        
        var executionPlan = new ExecutionPlan(state, package, name);
        foreach (var resourceDefinition in executionPlan.Resources)
        {
            var provider = providerRegistry.Get(resourceDefinition.ResourceType);
            if (provider == null)
            {
                logger.LogError("Can't find provider for resource {ProviderType}", resourceDefinition.ResourceType);
                return null;
            }

            executionPlan.Providers[resourceDefinition] = provider;
            executionPlan.Parameters[resourceDefinition] = provider.GetParameters(executionPlan, resourceDefinition);
        }

        return executionPlan;
    }
    
    public bool Validate(ExecutionPlan executionPlan)
    {
        if (state.Applications.Any(application => application.Name == executionPlan.Name))
        {
            return false;
        }
        
        foreach (var resourceDefinition in executionPlan.Resources)
        {
            var provider = executionPlan.Providers[resourceDefinition];
            if (!provider.Validate(executionPlan, executionPlan.Parameters[resourceDefinition]))
            {
                return false;
            }
        }

        return true;
    }
}