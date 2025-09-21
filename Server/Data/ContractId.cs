using System.Diagnostics;

namespace Frierun.Server.Data;

/// <summary>
/// Represents a contract ID - either a GUID for an installed contract or ContractRef for an uninstalled contract.
/// </summary>
public class ContractId<TContract> : ContractId where TContract : Contract
{
    public new ContractRef<TContract>? Ref => base.Ref != null ? new ContractRef<TContract>(base.Ref) : null;

    public ContractId(ContractRef<TContract> refId) : base(refId)
    {
    }

    public ContractId(Guid guid) : base(guid)
    {
    }
}

public class ContractId : IArgument
{
    public ContractRef? Ref { get; private set; }
    public Guid? Guid { get; private set; }

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

        Guid = plan.GetContract(Ref).Id;
        Ref = null;
    }

    public IEnumerable<ContractRef> RequiredContracts => Ref != null ? [Ref] : [];

    public object Merge(object other)
    {
        var contractId = (ContractId)other;
        var guid = Merger.OnlyOne(Guid, contractId.Guid);
        if (guid != null)
        {
            return new ContractId((Guid)guid);
        }

        var refId = Merger.OnlyOne(Ref, contractId.Ref);
        Debug.Assert(refId != null, "Contract ID must have either a GUID or a ContractRef");
        return new ContractId(refId);
    }
}