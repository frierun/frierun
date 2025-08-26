
using System.Diagnostics;

namespace Frierun.Server.Data;

public class ContractId<TContract> : ContractId
    where TContract : Contract
{
    public ContractId(string name) : base(typeof(TContract).Name, name)
    {
    }

    public ContractId(ContractId contractId) : base(typeof(TContract).Name, contractId.Name)
    {
        Debug.Assert(contractId.TypeName == typeof(TContract).Name);
    }
}

public class ContractId(
    string typeName,
    string name
) : IEquatable<ContractId>
{
    public string TypeName { get; } = typeName;
    public string Name { get; } = name;

    public override string ToString()
    {
        return $"{TypeName}:{Name}";
    }

    public bool Equals(ContractId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return TypeName == other.TypeName && Name == other.Name;
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

        if (obj is not ContractId contractId)
        {
            return false;
        }

        return Equals(contractId);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (TypeName.GetHashCode() * 397) ^ Name.GetHashCode();
        }
    }
    
    public static bool operator ==(ContractId? left, ContractId? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ContractId? left, ContractId? right)
    {
        return !(left == right);
    }
}