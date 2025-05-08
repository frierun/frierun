using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record ContractId<TContract>(
    string Name
) : ContractId(typeof(TContract), Name)
    where TContract : Contract
{
    public override string ToString()
    {
        return $"{Type.Name}:{Name}";
    }
}

public abstract record ContractId(
    [property: JsonIgnore] Type Type,
    string Name
)
{
    public virtual bool Equals(ContractId? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Type == other.Type && Name == other.Name;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Name);
    }
    
    public static ContractId Create(Type type, string name)
    {
        return (ContractId)Activator.CreateInstance(
            typeof(ContractId<>).MakeGenericType(type),
            name
        )!;
    }
}