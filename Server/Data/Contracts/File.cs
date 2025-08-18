
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record File(
    string Path,
    string? Name = null,
    Argument<string>? Text = null,
    ContractId<Volume>? Volume = null,
    int? Owner = null,
    int? Group = null
) : Contract(Name ?? $"{Path}{(Volume != null ? " in " + Volume.Name : "")}")
{
    public ContractId<Volume> Volume { get; init; } = Volume ?? new ContractId<Volume>("");
    public Argument<string> Text { get; init; } = Text ?? new Argument<string>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return Text;
    }

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            Path = OnlyOne(Path, contract.Path),
            Text = Text.Merge(contract.Text),
            Volume = OnlyOne(Volume, contract.Volume),
            Owner = OnlyOne(Owner, contract.Owner),
            Group = OnlyOne(Group, contract.Group)       
        };
    }
}