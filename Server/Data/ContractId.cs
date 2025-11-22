using System.Diagnostics;

namespace Frierun.Server.Data;

/// <summary>
/// Represents a contract ID - either a GUID for an installed contract or ContractRef for an uninstalled contract.
/// </summary>
public class ContractId<TContract> : ContractId where TContract : Contract
{
    public ContractRef<TContract> TypedRef => new(Ref ?? DefaultRef);
    public override ContractRef DefaultRef => new ContractRef<TContract>();

    public ContractId(Guid guid = default, string? name = null) :
        base(guid, name != null ? new ContractRef<TContract>(name) : null)
    {
    }
    
    public override object Merge(object other)
    {
        var contractId = (ContractId<TContract>)other;
        var contractRef = Merger.MergeValue(Ref, contractId.Ref);
        return new ContractId<TContract>(
            Merger.MergeValue(Guid, contractId.Guid),
            contractRef?.Name
        );
    }
}

public class ContractId(Guid guid = default, ContractRef? refId = null) : IArgument, IEquatable<ContractId>
{
    public ContractRef? Ref { get; } = refId;

    public virtual ContractRef DefaultRef =>
        throw new InvalidOperationException("Can't get default ref for untyped contract ID");

    public Guid Guid { get; protected set; } = guid;

    public static implicit operator ContractId(ContractRef refId) => new(Guid.Empty, refId);
    public static implicit operator ContractId(Guid guid) => new(guid);

    public virtual void Resolve(ExecutionPlan plan)
    {
        var contract = plan.GetContract(Ref ?? DefaultRef);
        if (!contract.Installed)
        {
            return;
        }

        if (Guid != Guid.Empty)
        {
            Debug.Assert(contract.Id == Guid, "Contract ID must match the GUID of the installed contract");
        }
        else
        {
            Guid = contract.Id;
        }
    }

    public IEnumerable<ContractRef> RequiredContracts => Ref != null ? [Ref] : [];

    public virtual object Merge(object other)
    {
        var contractId = (ContractId)other;
        return new ContractId(Merger.MergeValue(Guid, contractId.Guid), Merger.MergeValue(Ref, contractId.Ref));
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

        if (Guid != Guid.Empty || other.Guid != Guid.Empty)
        {
            return Equals(Guid, other.Guid);
        }

        return Equals(Ref, other.Ref);
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

        if (obj is not ContractId other)
        {
            return false;
        }

        return Equals(other);
    }
    
    public static bool operator ==(ContractId? left, ContractId? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ContractId? left, ContractId? right)
    {
        return !(left == right);
    }
    
    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        if (Guid != Guid.Empty)
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return Guid.GetHashCode();
        }
            
        return Ref != null ? Ref.GetHashCode() : DefaultRef.GetHashCode();
    }
}