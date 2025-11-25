namespace Frierun.Server.Data;

public class ContractResolver(ExecutionPlan plan) : IArgumentTransformer
{
    public T Transform<T>(T value) where T : IArgument
    {
        value.Resolve(plan);
        return value;
    }
}