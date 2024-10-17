using Frierun.Server.Resources;
using Newtonsoft.Json;

namespace Frierun.Server.Models;

public record State
{
    public IList<Application> Applications { get; init; } = [];

    [JsonIgnore]
    public IEnumerable<Resource> Resources =>
        Applications.SelectMany(application => application.Resources ?? Array.Empty<Resource>());
}