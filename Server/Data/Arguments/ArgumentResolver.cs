namespace Frierun.Server.Data;

public class ArgumentResolver<TResult>(Func<ExecutionPlan, TResult?> resolver) : IArgumentResolver<TResult>, IEquatable<ArgumentResolver<TResult>>
{
    private readonly Func<ExecutionPlan, TResult?> _resolver = resolver;

    public static implicit operator ArgumentResolver<TResult>(Func<ExecutionPlan, TResult?> value) => new(value);

    public TResult? Resolve(ExecutionPlan plan) => _resolver(plan);
    public IEnumerable<ContractRef> RequiredContracts => [];

    public bool Equals(ArgumentResolver<TResult>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return _resolver.Equals(other._resolver);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ArgumentResolver<TResult>)obj);
    }

    public override int GetHashCode()
    {
        return _resolver.GetHashCode();
    }
}

public class ArgumentResolver<T, TResult>(T parameter, Func<T, ExecutionPlan, TResult?> resolver)
    : IArgumentResolver<TResult>, IEquatable<ArgumentResolver<T, TResult>>
    where T : notnull
{
    private readonly T _parameter = parameter;
    private readonly Func<T, ExecutionPlan, TResult?> _resolver = resolver;

    public TResult? Resolve(ExecutionPlan plan)
    {
        return _resolver(_parameter, plan);
    }
    public IEnumerable<ContractRef> RequiredContracts => [];

    public bool Equals(ArgumentResolver<T, TResult>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityComparer<T>.Default.Equals(_parameter, other._parameter) &&
               _resolver.Equals(other._resolver);
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != GetType())
        {
            return false;
        }

        return Equals((ArgumentResolver<T, TResult>)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (EqualityComparer<T>.Default.GetHashCode(_parameter) * 397) ^ _resolver.GetHashCode();
        }
    }
}