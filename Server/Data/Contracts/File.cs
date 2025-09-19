
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record File(
    string Path,
    Argument<string>? Text = null,
    ContractRef<Volume>? Volume = null,
    int? Owner = null,
    int? Group = null
) : Contract
{
    public ContractRef<Volume> Volume { get; init; } = Volume ?? new ContractRef<Volume>("");
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