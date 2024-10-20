using System.Text.Json.Serialization;
using Frierun.Server.Resources;

namespace Frierun.Server.Models;

public record State
{
    public IList<Application> Applications { get; init; } = [];

    [JsonIgnore]
    public IEnumerable<Resource> Resources => Applications.SelectMany(application => application.AllResources);
}