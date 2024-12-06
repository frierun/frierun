namespace Frierun.Server.Data;

public record ResolvedParameter(
    string Name,
    string? Value = null
) : Resource;