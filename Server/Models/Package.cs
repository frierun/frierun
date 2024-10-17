using Frierun.Server.Resources;

namespace Frierun.Server.Models;

public record Package(
    string Name,
    string Url,
    IList<ResourceDefinition> Resources
);