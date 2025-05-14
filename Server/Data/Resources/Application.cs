using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;


public class Application : Resource
{
    [JsonConstructor]
    protected Application(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }
    
    public Application(IHandler handler) : base(handler)
    {
    }

    public required string Name { get; init; }
    public Package? Package { get; init; }
    public string? Url { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<Contract> Contracts { get; init; } = Array.Empty<Contract>();
    [JsonIgnore] public IEnumerable<Resource> Resources => Contracts
        .OfType<IHasResult>()
        .Select(contract => contract.Result)
        .OfType<Resource>();
    public IReadOnlyList<string> RequiredApplications { get; init; } = Array.Empty<string>();
}