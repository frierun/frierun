namespace Frierun.Server.Data;

public interface IArgumentTransformer
{
    T Transform<T>(T value) where T : IArgument;
}