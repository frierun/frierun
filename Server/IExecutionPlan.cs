using Frierun.Server.Data;

namespace Frierun.Server;

public interface IExecutionPlan
{
    /// <summary>
    /// Installs all contracts in the execution plan.
    /// </summary>
    public Application Install();
}