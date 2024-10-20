namespace Frierun.Server.Resources;

public record HttpEndpoint(Guid Id, string Url) : Resource(Id, new List<Resource>());
