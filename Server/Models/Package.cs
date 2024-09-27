namespace Frierun.Server.Models;

public record Package(
    string Name,
    string ImageName,
    int Port,
    IList<string>? OldVolumes = null,
    bool RequireDocker = false
);