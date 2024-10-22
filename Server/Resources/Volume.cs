namespace Frierun.Server.Resources;

public record Volume(Guid Id, string Name, IReadOnlyList<Resource> Children) : Resource(Id, Children);