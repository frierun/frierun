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
    IArgument Resolve(ExecutionPlan plan);
    IEnumerable<ContractId> RequiredContracts { get; }
    object Merge(object other);
}