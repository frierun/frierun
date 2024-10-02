namespace Frierun.Server.Models;

public record Application(
    Guid Id,
    string Name,
    int Port,
    IList<string>? VolumeNames = null,
    Package? Package = null
);