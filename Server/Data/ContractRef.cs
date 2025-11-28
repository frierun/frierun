
using System.Diagnostics;

namespace Frierun.Server.Data;

public class ContractRef<TContract> : ContractRef
    where TContract : Contract
{
    public ContractRef(string? name = null) : base(typeof(TContract).Name, name ?? "")
    {
    }

    public ContractRef(ContractRef contractRef) : base(typeof(TContract).Name, contractRef.Name)
    {
        Debug.Assert(contractRef.TypeName == typeof(TContract).Name);
    }
}

public class ContractRef(
    string typeName,
    string name
) : IEquatable<ContractRef>
{
    public string TypeName { get; } = typeName;
    public string Name { get; } = name;

    public override string ToString()
    {
        return $"{TypeName}:{Name}";
    }

    public bool Equals(ContractRef? other)
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

        if (obj is not ContractRef contractRef)
        {
            return false;
        }

        return Equals(contractRef);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (TypeName.GetHashCode() * 397) ^ Name.GetHashCode();
        }
    }
    
    public static bool operator ==(ContractRef? left, ContractRef? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ContractRef? left, ContractRef? right)
    {
        return !(left == right);
    }
}