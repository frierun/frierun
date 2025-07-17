
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record File(
    string Path,
    string? Name = null,
    string? Text = null,
    ContractId<Volume>? Volume = null,
    int? Owner = null,
    int? Group = null
) : Contract(Name ?? $"{Path}{(Volume != null ? " in " + Volume.Name : "")}"), IHasStrings
{
    public ContractId<Volume> Volume { get; init; } = Volume ?? new ContractId<Volume>("");
    
    Contract IHasStrings.ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Text = Text == null ? null : decorator(Text),
        };
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Path = OnlyOne(Path, contract.Path),
            Text = OnlyOne(Text, contract.Text),
            Volume = OnlyOne(Volume, contract.Volume),
            Owner = OnlyOne(Owner, contract.Owner),
            Group = OnlyOne(Group, contract.Group)       
        };
    }
}