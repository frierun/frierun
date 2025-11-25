namespace Frierun.Server.Data;

public class ArgumentCounter : IArgumentTransformer
{
    private readonly IList<IArgument> _arguments = [];
    public IEnumerable<IArgument> Arguments => _arguments;
    
    public T Transform<T>(T value) where T : IArgument
    {
        _arguments.Add(value);
        return value;
    }
}