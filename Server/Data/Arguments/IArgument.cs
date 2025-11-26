namespace Frierun.Server.Data;

public interface IArgument<TSelf> : IArgument where TSelf : IArgument<TSelf>
{
    new TSelf Resolve(ExecutionPlan plan);

    IArgument IArgument.Resolve(ExecutionPlan plan)
    {
        return Resolve(plan);
    }
}

public interface IArgument
{
    /// <summary>
    /// Resolves the argument using ExecutionPlan.
    /// </summary>
    IArgument Resolve(ExecutionPlan plan);
    
    /// <summary>
    /// List of Contracts that must exist for this argument to be resolved.
    /// </summary>
    IEnumerable<ContractRef> RequiredContracts { get; }
    
    /// <summary>
    /// Merges two arguments.
    /// </summary>
    object Merge(object other);
}