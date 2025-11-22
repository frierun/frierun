namespace Frierun.Server.Data;

public static class Merger
{
    /// <summary>
    /// Merges common part of the contract.
    /// </summary>
    public static TContract MergeCommon<TContract>(TContract contract, Contract other, out TContract castOther)
        where TContract : Contract
    {
        if (other is not TContract cast)
        {
            throw new MergeException("Merge different types");
        }

        castOther = cast;

        var result = contract with
        {
            Id = MergeValue(contract.Id, other.Id),
            Handler = MergeValue(contract.Handler, other.Handler),
            HandlerApplication = MergeValue(contract.HandlerApplication, other.HandlerApplication),
            DependsOn = contract.DependsOn.Concat(other.DependsOn).Distinct()
        };

        if (result is { Handler: not null, HandlerApplication: not null } &&
            result.Handler.Application?.Name != result.HandlerApplication)
        {
            throw new MergeException("Handler application does not match HandlerApplication restriction");
        }

        return result;
    }

    /// <summary>
    /// Ensures that only one of the provided values is set and returns it.
    /// </summary>
    public static T MergeValue<T>(T value1, T value2, Func<T, bool>? isUnset = null)
    {
        isUnset ??= value => Equals(value, default(T));

        if (isUnset(value1))
        {
            return value2;
        }

        if (isUnset(value2))
        {
            return value1;
        }

        if (value1?.Equals(value2) == true)
        {
            return value1;
        }

        throw new MergeException("Can't merge two different values");
    }

    /// <summary>
    /// Merges two typed contract ids
    /// </summary>
    public static ContractId<T> MergeContractId<T>(ContractId<T> value1, ContractId<T> value2) where T : Contract
    {
        return (ContractId<T>)value1.Merge(value2);
    }

    /// <summary>
    /// Merges two dictionaries.
    /// </summary>
    public static Dictionary<TKey, TValue> MergeDictionary<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> dict1,
        IEnumerable<KeyValuePair<TKey, TValue>> dict2
    ) where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>(dict1);
        foreach (var (key, value2) in dict2)
        {
            if (value2 is null)
            {
                continue;
            }

            if (!result.TryGetValue(key, out var value1) || value1 is null)
            {
                result[key] = value2;
                continue;
            }

            if (value1.Equals(value2))
            {
                continue;
            }

            if (value1 is IArgument argument)
            {
                result[key] = (TValue)argument.Merge(value2);
                continue;
            }

            throw new MergeException("Can't merge two different values");
        }

        return result;
    }

    /// <summary>
    /// Checks if the other contract is fulfilling the contract. Also casts it to the type of the contract.
    /// </summary>
    public static bool IsSubsetContract<TContract>(TContract contract, Contract other, out TContract castOther)
        where TContract : Contract
    {
        if (other is not TContract cast)
        {
            castOther = contract;
            return false;
        }

        castOther = cast;

        if (other.HandlerApplication != null && other.HandlerApplication != contract.Handler?.Application?.Name)
        {
            return false;
        }

        if (other.Handler != null && other.Handler != contract.Handler)
        {
            return false;
        }

        if (other.Id != Guid.Empty && other.Id != contract.Id)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Ensures that other value is not set or equal to the value.
    /// </summary>
    public static bool IsSubsetValue<T>(T value, T other, Func<T, bool>? isUnset = null)
    {
        isUnset ??= t => Equals(t, default(T));

        if (isUnset(other))
        {
            return true;
        }

        if (value?.Equals(other) == true)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Ensures that other value is not set or equal to the value. 
    /// </summary>
    public static bool IsSubsetArgument<T>(Argument<T> value, Argument<T> other)
    {
        if (!other.Resolved || !value.Resolved)
        {
            return false;
        }

        if (value.Value is null)
        {
            return other.Value is null;
        }

        return value.Value.Equals(other.Value);
    }

    /// <summary>
    /// Ensures that the value list contains all elements from the other list.
    /// </summary>
    public static bool IsSubsetList<T>(IEnumerable<T> value, IEnumerable<T> other)
    {
        var hashSet = new HashSet<T>(value);
        return other.All(otherValue => hashSet.Contains(otherValue));
    }

    /// <summary>
    /// Ensures that the value dictionary contains all elements from the other dictionary.
    /// </summary>
    public static bool IsSubsetDictionary<TKey, TValue>(
        IReadOnlyDictionary<TKey, TValue> value,
        IReadOnlyDictionary<TKey, TValue> other
    ) where TKey : notnull
    {
        foreach (var pair in other)
        {
            if (!value.TryGetValue(pair.Key, out var storedValue))
            {
                return false;
            }

            if (storedValue == null)
            {
                if (pair.Value != null)
                {
                    return false;
                }

                continue;
            }

            if (!storedValue.Equals(pair.Value))
            {
                return false;
            }
        }

        return true;
    }
}