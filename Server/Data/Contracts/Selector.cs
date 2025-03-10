namespace Frierun.Server.Data;

public record SelectorOption(string Name, List<Contract> Contracts);

public record Selector(
    string Name,
    List<SelectorOption> Options,
    string? SelectedOption = null
) : Contract(Name ?? "")
{
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