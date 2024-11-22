namespace Frierun.Server.Data;

public record Package(
    string Name,
    string ?Prefix = null,
    string? Url = null,
    IReadOnlyList<Contract>? Contracts = null
) : Contract<Application>(Name)
{
    public IReadOnlyList<Contract> Contracts { get; init; } = Contracts ?? Array.Empty<Contract>();
}