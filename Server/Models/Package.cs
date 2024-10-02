namespace Frierun.Server.Models;

public record Package(
    string Name,
    string ImageName,
    int Port,
    IList<Volume>? Volumes = null,
    bool RequireDocker = false
);