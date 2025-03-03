namespace Frierun.Server.Data;

public record SelectorOption(string Name, List<Contract> Contracts);

public record Selector(
    string Name,
    List<SelectorOption> Options,
    string? SelectedOption = null
) : Contract(Name ?? "");