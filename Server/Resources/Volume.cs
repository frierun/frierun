namespace Frierun.Server.Resources;

public record Volume(Guid Id, string Name) : Resource(Id, new List<Resource>());