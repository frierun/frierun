using System.Diagnostics;

namespace Frierun.Server.Data;

public class Argument<T> : IEquatable<Argument<T>>, IArgument
{
    private bool _resolving;

    public Argument()
    {
        Value = default;
    }

    public Argument(T? value)
    {
        if (value == null || value is not string || !ApplyTemplate(value.ToString() ?? ""))
        {
            Value = value;
        }
    }

    public Argument(Func<ExecutionPlan, T?> resolver)
    {
        Resolver = new ArgumentResolver<T>(resolver);
    }
    
    public Argument(IArgumentResolver<T> resolver)
    {
        Resolver = resolver;
    }

    public static implicit operator Argument<T>(T? value) => new(value);
    public static implicit operator Argument<T>(Func<ExecutionPlan, T?> resolver) => new(resolver);
    public static implicit operator T?(Argument<T> arg) => arg.Value;

    /// <summary>
    /// Resolved value of argument. Maybe default if not resolved yet.
    /// </summary>
    public T? Value { get; private set; }

    /// <summary>
    /// Function to resolve argument
    /// </summary>
    public IArgumentResolver<T>? Resolver { get; private set; }

    /// <summary>
    /// List of contracts which are required to resolve value
    /// </summary>
    public IEnumerable<ContractRef> RequiredContracts => Resolver?.RequiredContracts ?? [];

    /// <summary>
    /// Checks if the argument is resolved
    /// </summary>
    public bool Resolved => !Equals(Value, default(T));

    /// <summary>
    /// Checks if the argument is empty, that is it has neither value, nor resolver
    /// </summary>
    public bool Empty => !Resolved && Resolver == null;

    /// <summary>
    /// Creates a resolver to all variables in templates in form {{Contract:Name:Argument}} 
    /// </summary>
    /// <returns>True if the resolver is set, false if it is not required</returns>
    private bool ApplyTemplate(string value)
    {
        Resolver = (IArgumentResolver<T>?)TemplateResolver.Create(value);
        return Resolver != null;
    }

    /// <summary>
    /// Resolves the real value of argument
    /// </summary>
    public void Resolve(ExecutionPlan plan)
    {
        if (Resolver == null)
        {
            return;
        }

        Debug.Assert(!Resolved, "Can't resolve already resolved argument");

        if (_resolving)
        {
            throw new Exception("Can't resolve argument recursively");
        }

        _resolving = true;
        Value = Resolver.Resolve(plan);
        _resolving = false;
        Resolver = null;
    }

    /// <summary>
    /// Merges two arguments and returns a new one
    /// </summary>
    public Argument<T> Merge(Argument<T> other)
    {
        var merged = new Argument<T>
        {
            Value = Merger.MergeValue(Value, other.Value),
            Resolver = Merger.MergeValue(Resolver, other.Resolver),
        };

        if (merged is { Resolved: true, Resolver: not null })
        {
            throw new MergeException("Can't merge two different values");
        }

        return merged;
    }

    object IArgument.Merge(object other)
    {
        return Merge((Argument<T>)other);
    }

    public bool Equals(Argument<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return EqualityComparer<T?>.Default.Equals(Value, other.Value) && Equals(Resolver, other.Resolver);
    }

    public override string? ToString()
    {
        if (Resolved && Value is not null)
        {
            return Value.ToString();
        }

        return "Unresolved";
    }
}