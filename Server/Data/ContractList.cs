using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Frierun.Server.Handlers;

namespace Frierun.Server.Data;

[CollectionBuilder(typeof(ContractList), "Create")]
public class ContractList : IReadOnlyDictionary<ContractId, Contract>
{
    private readonly Dictionary<ContractId, Contract> _contracts;

    public static ContractList Create(ReadOnlySpan<KeyValuePair<ContractId, Contract>> contracts)
    {
        return new ContractList(contracts.ToArray());
    }

    public ContractList()
    {
        _contracts = [];
    }

    public ContractList(IDictionary<ContractId, Contract> contracts)
    {
        _contracts = new Dictionary<ContractId, Contract>(contracts);
    }

    public ContractList(IEnumerable<KeyValuePair<ContractId, Contract>> contracts)
    {
        _contracts = new Dictionary<ContractId, Contract>(contracts);
    }
    
    public ContractList(ContractList contracts)
    {
        _contracts = new Dictionary<ContractId, Contract>(contracts);
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<ContractId, Contract>> GetEnumerator()
    {
        return _contracts.GetEnumerator();
    }

    public int Count => _contracts.Count;

    public bool ContainsKey(ContractId key)
    {
        return _contracts.ContainsKey(key);
    }

    public bool TryGetValue(ContractId key, [MaybeNullWhen(false)] out Contract value)
    {
        return _contracts.TryGetValue(key, out value);
    }

    public Contract this[ContractId key]
    {
        get => _contracts[key];
        init => _contracts[key] = value;
    }
    
    public Contract this[ApplicationContext context]
    {
        init => _contracts[new ContractId(value.GetType().Name, context.Name)] = value;
    }

    public Contract this[string name]
    {
        init => _contracts[new ContractId(value.GetType().Name, name)] = value;
    }

    public IEnumerable<ContractId> Keys => _contracts.Keys;

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
                    group.Aggregate((a, b) => new KeyValuePair<ContractId, Contract>(a.Key, a.Value.Merge(b.Value)))
                )
        );
    }
}