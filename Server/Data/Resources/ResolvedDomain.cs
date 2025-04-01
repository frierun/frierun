namespace Frierun.Server.Data;

public record ResolvedDomain(
    string Value,
    bool IsInternal
) : Resource;