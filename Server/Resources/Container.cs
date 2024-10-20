namespace Frierun.Server.Resources;

public record Container(Guid Id, string Name, IReadOnlyList<Resource> Children) : Resource(Id, Children);
