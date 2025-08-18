namespace Frierun.Server.Data;

public interface IArgument
{
    void Resolve(ExecutionPlan plan);
    IEnumerable<ContractId> RequiredContracts { get; }
    object Merge(object other);
}