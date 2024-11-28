using System.Text.Json.Serialization;

namespace Frierun.Server.Data;

public record ContractId<TContract>(
    string Name
) : ContractId(typeof(TContract), Name)
    where TContract : Contract;

public record ContractId(
    [property: JsonIgnore] Type Type,
    string Name
)
{
    /// <inheritdoc />
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

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(Type, Name);
    }
}