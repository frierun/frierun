namespace Frierun.Server.Data;

public interface IArgument
{
    void Resolve(ExecutionPlan plan);
    IEnumerable<ContractRef> RequiredContracts { get; }
    object Merge(object other);
}