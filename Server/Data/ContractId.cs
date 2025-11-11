using System.Diagnostics;

namespace Frierun.Server.Data;

/// <summary>
/// Represents a contract ID - either a GUID for an installed contract or ContractRef for an uninstalled contract.
/// </summary>
public class ContractId<TContract> : ContractId where TContract : Contract
{
    public new ContractRef<TContract>? Ref => base.Ref != null ? new ContractRef<TContract>(base.Ref) : null;

    public ContractId(string name) : base(new ContractRef<TContract>(name))
    {
    }

    public ContractId(Guid guid) : base(guid)
    {
    }

    public override object Merge(object other)
    {
        var contractId = (ContractId<TContract>)other;
        var guid = Merger.MergeValue(Guid, contractId.Guid);
        if (guid != null)
        {
            return new ContractId((Guid)guid);
        }

        var refId = Merger.MergeValue(Ref, contractId.Ref);
        Debug.Assert(refId != null, "Contract ID must have either a GUID or a ContractRef");
        return new ContractId<TContract>(refId.Name);
    }
}

public class ContractId : IArgument, IEquatable<ContractId>
{
    public ContractRef? Ref { get; private set; }
    public Guid? Guid { get; private set; }

    public static implicit operator ContractId(ContractRef refId) => new(refId);
    public static implicit operator ContractId(Guid guid) => new(guid);

    public ContractId(ContractRef refId)
    {
        Ref = refId;
    }

    public ContractId(Guid guid)
    {
        Guid = guid;
    }

    public void Resolve(ExecutionPlan plan)
    {
        if (Guid != null)
        {
            return;
        }

        Debug.Assert(Ref != null, "Contract ID must have either a GUID or a ContractRef");

        var contract = plan.GetContract(Ref);
        if (!contract.Installed)
        {
            return;
        }
        
        Guid = contract.Id;
        Ref = null;
    }

    public IEnumerable<ContractRef> RequiredContracts => Ref != null ? [Ref] : [];

    public virtual object Merge(object other)
    {
        var contractId = (ContractId)other;
        var guid = Merger.MergeValue(Guid, contractId.Guid);
        if (guid != null)
        {
            return new ContractId((Guid)guid);
        }

        var refId = Merger.MergeValue(Ref, contractId.Ref);
        Debug.Assert(refId != null, "Contract ID must have either a GUID or a ContractRef");
        return new ContractId(refId);
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

        if (Guid != null)
        {
            return Equals(Guid, other.Guid);
        }

        return Equals(Ref, other.Ref);
    }
}