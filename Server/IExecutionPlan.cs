using Frierun.Server.Data;

namespace Frierun.Server;

public interface IExecutionPlan
{
    /// <summary>
    /// Installs all contracts in the execution plan.
    /// </summary>
    /// <param name="state"></param>
    public Application Install(State state);
}