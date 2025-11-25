namespace Frierun.Server.Data;

public interface IArgumentResolver<out TResult>
{
    TResult? Resolve(ExecutionPlan plan);
    IEnumerable<ContractId> RequiredContracts { get; }
}