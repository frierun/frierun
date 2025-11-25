
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

    public override File Transform(IArgumentTransformer transformer)
    {
        return this with
        {
            Text = transformer.Transform(Text),
            DependsOn = DependsOn.Select(transformer.Transform)
        };
    }
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            Path = MergeValue(Path, contract.Path),
            Text = Text.Merge(contract.Text),
            Volume = MergeValue(Volume, contract.Volume),
            Owner = MergeValue(Owner, contract.Owner),
            Group = MergeValue(Group, contract.Group)       
        };
    }
}