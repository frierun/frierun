namespace Frierun.Server.Data;

public record NetworkContract(
    string Name,
    string? NetworkName = null
) : Contract<Network>(Name);
