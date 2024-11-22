using Frierun.Server.Data;

namespace Frierun.Server.Services;

public class ExecutionService(
    ProviderRegistry providerRegistry,
    State state
)
{
    /// <summary>
    /// Creates an execution plan for the given package.
    /// </summary>
    public ExecutionPlan Create(Package package)
    {
        var plan = new ExecutionPlan(package, state, providerRegistry);
        return plan;
    }
}