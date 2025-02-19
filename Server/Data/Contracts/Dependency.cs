namespace Frierun.Server.Data;

public record Dependency(
    ContractId Preceding,
    ContractId Following
) : Contract($"{Preceding} -> {Following}");