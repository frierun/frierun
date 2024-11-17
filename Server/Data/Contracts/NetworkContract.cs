namespace Frierun.Server.Data;

public record NetworkContract(
    string Name
) : Contract<Network>(Name);
