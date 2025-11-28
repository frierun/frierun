namespace Frierun.Server.Data;

public interface IArgumentResolver<out TResult>
{
    /// <summary>
    /// Resolves the argument using ExecutionPlan.
    /// </summary>
    TResult? Resolve(ExecutionPlan plan);
    
    /// <summary>
    /// List of Contracts that must exist for this argument to be resolved.
    /// </summary>
    IEnumerable<ContractRef> RequiredContracts { get; }
}