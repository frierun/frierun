using System.Diagnostics;

namespace Frierun.Server.Data;

/// <summary>
/// Represents a contract ID - either a GUID for an installed contract or ContractRef for an uninstalled contract.
/// </summary>
public class ContractId<TContract> : ContractId where TContract : Contract
{
    public new ContractRef<TContract>? Ref => base.Ref != null ? new ContractRef<TContract>(base.Ref) : null;

    public ContractId(Guid guid = default, string? name = null) :
        base(guid, name != null ? new ContractRef<TContract>(name) : null)
    {
    }

    public override void Resolve(ExecutionPlan plan)
    {
        if (Ref == null && Guid == Guid.Empty)
        {
            base.Ref = new ContractRef<TContract>("");
        }

        base.Resolve(plan);
    }

    public override object Merge(object other)
    {
        var contractId = (ContractId<TContract>)other;
        return new ContractId<TContract>(
            Merger.MergeValue(Guid, contractId.Guid),
            Merger.MergeValue(Ref, contractId.Ref)?.Name
        );
    }
}

public class ContractId(Guid guid = default, ContractRef? refId = null) : IArgument, IEquatable<ContractId>
{
    public ContractRef? Ref { get; protected set; } = refId;
    public Guid Guid { get; protected set; } = guid;

    public static implicit operator ContractId(ContractRef refId) => new(Guid.Empty, refId);
    public static implicit operator ContractId(Guid guid) => new(guid);

    public virtual void Resolve(ExecutionPlan plan)
    {
        if (Ref == null)
        {
            Debug.Assert(Guid != Guid.Empty, "Contract ID must have either a GUID or a ContractRef");
            return;
        }

        var contract = plan.GetContract(Ref);
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

        Ref = null;
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

        if (Guid != Guid.Empty)
        {
            return Equals(Guid, other.Guid);
        }

        return Equals(Ref, other.Ref);
    }
}