using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Frierun.Server.Handlers;

namespace Frierun.Server.Data;

[CollectionBuilder(typeof(ContractList), "Create")]
public class ContractList : IReadOnlyDictionary<ContractRef, Contract>
{
    private readonly Dictionary<ContractRef, Contract> _contracts;

    public static ContractList Create(ReadOnlySpan<KeyValuePair<ContractRef, Contract>> contracts)
    {
        return new ContractList(contracts.ToArray());
    }

    public ContractList()
    {
        _contracts = [];
    }

    public ContractList(IDictionary<ContractRef, Contract> contracts)
    {
        _contracts = new Dictionary<ContractRef, Contract>(contracts);
    }

    public ContractList(IEnumerable<KeyValuePair<ContractRef, Contract>> contracts)
    {
        _contracts = new Dictionary<ContractRef, Contract>(contracts);
    }

    public ContractList(ContractList contracts)
    {
        _contracts = new Dictionary<ContractRef, Contract>(contracts);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<ContractRef, Contract>> GetEnumerator()
    {
        return _contracts.GetEnumerator();
    }

    public int Count => _contracts.Count;

    public bool ContainsKey(ContractRef key)
    {
        return _contracts.ContainsKey(key);
    }

    public bool TryGetValue(ContractRef key, [MaybeNullWhen(false)] out Contract value)
    {
        return _contracts.TryGetValue(key, out value);
    }

    public Contract this[ContractRef key]
    {
        get => _contracts[key];
        init => _contracts[key] = value;
    }

    public Contract this[ApplicationContext context]
    {
        init => _contracts[new ContractRef(value.GetType().Name, context.Name)] = value;
    }

    public Contract this[string name]
    {
        init => _contracts[new ContractRef(value.GetType().Name, name)] = value;
    }

    public IEnumerable<ContractRef> Keys => _contracts.Keys;

    public IEnumerable<Contract> Values => _contracts.Values;

    /// <summary>
    /// Merges two contract lists.
    /// </summary>
    public ContractList Merge(ContractList other)
    {
        return new ContractList(
            this
                .Concat(other)
                .GroupBy(c => c.Key)
                .Select(group =>
                    group.Aggregate((a, b) =>
                        new KeyValuePair<ContractRef, Contract>(a.Key, Merger.Merge(a.Value, b.Value))
                    )
                )
        );
    }
}