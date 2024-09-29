namespace Frierun.Server.Models;

public record Application(Guid Id, string Name, int Port, Package Package);