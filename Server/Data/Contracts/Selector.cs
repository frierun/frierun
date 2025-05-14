namespace Frierun.Server.Data;

public record SelectorOption(string Name, List<Contract> Contracts);

public record Selector(
    string Name,
    IList<SelectorOption>? Options = null,
    string? SelectedOption = null,
    ResolvedParameter? Result = null
) : Contract(Name ?? ""), IHasResult<ResolvedParameter>
{
    public IList<SelectorOption> Options { get; init; } = Options ?? new List<SelectorOption>();
    
    public override Contract With(Contract other)
    {
        if (other is not Selector selector || other.Id != Id)
        {
            throw new Exception("Invalid contract");
        }

        return this with
        {
            SelectedOption = selector.SelectedOption
        };
    }
}