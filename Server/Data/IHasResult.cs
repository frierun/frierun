namespace Frierun.Server.Data;

public interface IHasResult<out TResource> : IHasResult
    where TResource : Resource
{
    new TResource? Result { get; }
    Resource? IHasResult.Result => Result;
}

public interface IHasResult
{
    Resource? Result { get; }
}