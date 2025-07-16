using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;

namespace Frierun.Server.Data;

public static class Merger
{
    /// <summary>
    /// Ensures that the provided contract is the same type and id as the provided one.
    /// </summary>
    public static TContract EnsureSame<TContract>(TContract contract, Contract other) where TContract : Contract
    {
        if (other is not TContract t || other.Id != contract.Id)
        {
            throw new MergeException("Invalid contract");
        }

        return t;
    }

    /// <summary>
    /// Merges common part of the contract.
    /// </summary>
    public static TContract MergeCommon<TContract>(TContract contract, Contract other) where TContract : Contract
    {
        if (contract.Installed || other.Installed)
        {
            throw new MergeException("Can't merge installed contracts");
        }

        return contract with
        {
            Name = OnlyOne(contract.Name, other.Name),
            Handler = OnlyOne(contract.Handler, other.Handler),
            DependsOn = contract.DependsOn.Concat(other.DependsOn),
            DependencyOf = contract.DependencyOf.Concat(other.DependencyOf),
        };
    }

    /// <summary>
    /// Ensures that only one of the provided values is set and returns it.
    /// </summary>
    public static T OnlyOne<T>(T value1, T value2, Func<T, bool>? isUnset = null)
    {
        isUnset ??= value => value is null;

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

    public static Dictionary<TKey, TValue> MergeDictionaries<TKey, TValue>(
        IEnumerable<KeyValuePair<TKey, TValue>> dict1,
        IEnumerable<KeyValuePair<TKey, TValue>> dict2
    ) where TKey : notnull
    {
        var result = new Dictionary<TKey, TValue>(dict1);
        foreach (var (key, value2) in dict2)
        {
            if (!result.TryGetValue(key, out var value1) || value1 is null)
            {
                result[key] = value2;
                continue;
            }
            
            if (value1.Equals(value2))
            {
                continue;
            }
            
            throw new MergeException("Can't merge two different values");
        }

        return result;
    }
}