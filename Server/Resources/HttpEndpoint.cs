namespace Frierun.Server.Resources;

public record HttpEndpoint(Guid Id, int Port) : Resource(Id, new List<Resource>());
