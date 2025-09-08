using System.Collections;
using System.Diagnostics.CodeAnalysis;
using Frierun.Server.Handlers;

namespace Frierun.Server.Data;

public class ContractList : IReadOnlyDictionary<ContractId, Contract>, IEnumerable<Contract>
{
    private readonly Dictionary<ContractId, Contract> _contracts;

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

    public ContractList(IEnumerable<Contract> contracts)
    {
        _contracts =
            new Dictionary<ContractId, Contract>(
                contracts.Select(c => new KeyValuePair<ContractId, Contract>(c.Id, c))
            );
    }

    public ContractList(ContractList contracts)
    {
        _contracts = new Dictionary<ContractId, Contract>(contracts);
    }
    
    public void Add(KeyValuePair<ContractId, Contract> pair)
    {
        _contracts.Add(pair.Key, pair.Value);
    }
    
    public IEnumerator<Contract> GetEnumerator()
    {
        return _contracts.Values.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator<KeyValuePair<ContractId, Contract>> IEnumerable<KeyValuePair<ContractId, Contract>>.GetEnumerator()
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
}