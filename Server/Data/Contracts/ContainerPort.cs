namespace Frierun.Server.Data;

public record ContainerPort(int InternalPort, int ExternalPort, Protocol Protocol);