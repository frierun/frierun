namespace Frierun.Server.Data;

public class UriArgument(Argument<bool?> ssl, Argument<string> host, Argument<int> port) : IArgument
{
    public Uri Value => new($"http{(ssl == true ? "s" : "")}://{host}:{port}");

    public void Resolve(ExecutionPlan plan)
    {
        ssl.Resolve(plan);
        host.Resolve(plan);
        port.Resolve(plan);
    }

    public IEnumerable<ContractId> RequiredContracts => [];

    public object Merge(object other)
    {
        throw new NotImplementedException();
    }

    public override string ToString()
    {
        return Value.ToString();
    }
}