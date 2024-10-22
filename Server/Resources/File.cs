namespace Frierun.Server.Resources;

public record File(Guid Id) : Resource(Id, new List<Resource>());
