using System.Text.RegularExpressions;

namespace Frierun.Server.Data;

public static class Argument
{
    public static readonly Regex InsertionRegex = new(@"{{([^}]+)}}", RegexOptions.Compiled);
    public static readonly Regex VariableRegex = new(@"^(\w+):([\w/ ]*):(\w+)$", RegexOptions.Compiled);
}

public class Argument<T> : IEquatable<Argument<T>>, IArgument
{
    public T? Value { get; private set; }
    public Func<ExecutionPlan, T>? Resolver { get; private set; }
    public IEnumerable<ContractId> RequiredContracts { get; private set; } = [];
    private bool Resolved => !Equals(Value, default(T));
    public bool Empty => !Resolved && Resolver == null;

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

    public Argument(Func<ExecutionPlan, T> resolver)
    {
        Resolver = resolver;
    }

    public static implicit operator Argument<T>(T? value) => new(value);
    public static implicit operator T?(Argument<T> arg) => arg.Value;

    /// <summary>
    /// Creates a resolver to all variables in templates in form {{Contract:Name:Argument}} 
    /// </summary>
    /// <returns>True if the resolver is set, false if it is not required</returns>
    private bool ApplyTemplate(string value)
    {
        var matchCollection = Argument.InsertionRegex.Matches(value);
        if (matchCollection.Count == 0)
        {
            return false;
        }

        RequiredContracts = matchCollection
            .Select(match => match.Groups[1].Value)
            .Select(insertion =>
                {
                    var match = Argument.VariableRegex.Match(insertion);
                    if (!match.Success)
                    {
                        throw new Exception($"Invalid insertion format: {insertion}");
                    }

                    var contractTypeName = match.Groups[1].Value;
                    var contractType = ContractRegistry.GetContractType(contractTypeName);
                    var contractName = match.Groups[2].Value;

                    return ContractId.Create(contractType, contractName);
                }
            )
            .ToList();

        Resolver = plan =>
        {
            foreach (Match match in matchCollection)
            {
                var propertyValue = ResolveInsertion(match.Groups[1].Value, plan);
                value = value.Replace($"{{{{{match.Groups[1].Value}}}}}", propertyValue);
            }

            return (T)Convert.ChangeType(value, typeof(T));
        };

        return true;
    }

    /// <summary>
    /// Resolves insertion value.
    /// </summary>
    private static string ResolveInsertion(string insertion, ExecutionPlan plan)
    {
        var match = Argument.VariableRegex.Match(insertion);
        if (!match.Success)
        {
            throw new Exception($"Invalid insertion format: {insertion}");
        }

        var contractTypeName = match.Groups[1].Value;
        var contractName = match.Groups[2].Value;
        var propertyName = match.Groups[3].Value;

        var contractType = ContractRegistry.GetContractType(contractTypeName);
        var contractId = ContractId.Create(contractType, contractName);

        var result = plan.GetContract(contractId);

        var propertyInfo = result.GetType().GetProperty(propertyName);
        if (propertyInfo == null)
        {
            throw new Exception($"Property not found: {propertyName} in {contractType}");
        }

        return propertyInfo.GetValue(result)?.ToString() ?? "";
    }

    public void Resolve(ExecutionPlan plan)
    {
        if (Resolver == null)
        {
            return;
        }

        Value = Resolver.Invoke(plan);
        Resolver = null;
    }

    /// <summary>
    /// Merges two arguments and returns a new one
    /// </summary>
    public Argument<T> Merge(Argument<T> other)
    {
        var merged = new Argument<T>
        {
            Value = Merger.OnlyOne(Value, other.Value),
            Resolver = Merger.OnlyOne(Resolver, other.Resolver),
            RequiredContracts = RequiredContracts.Concat(other.RequiredContracts)
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
}