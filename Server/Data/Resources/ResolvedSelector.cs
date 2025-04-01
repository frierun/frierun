namespace Frierun.Server.Data;

public record ResolvedSelector(
    string Name,
    string? Value = null
) : Resource;