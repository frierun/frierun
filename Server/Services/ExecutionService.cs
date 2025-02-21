using Frierun.Server.Data;

namespace Frierun.Server.Services;

public class ExecutionService(
    InstallerRegistry installerRegistry,
    State state
)
{
    /// <summary>
    /// Creates an execution plan for the given package.
    /// </summary>
    public ExecutionPlan Create(Package package)
    {
        return new ExecutionPlan(package, state, installerRegistry);
    }
}