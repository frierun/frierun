namespace Frierun.Server.Data;

public class ContractResolver(ExecutionPlan plan) : IArgumentTransformer
{
    public T Transform<T>(T value) where T : IArgument
    {
        return (T)value.Resolve(plan);
    }
}