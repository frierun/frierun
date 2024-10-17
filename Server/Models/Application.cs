using Frierun.Server.Resources;

namespace Frierun.Server.Models;

public record Application(
    Guid Id,
    string Name,
    IReadOnlyList<Resource>? Resources = null,
    Package? Package = null
);