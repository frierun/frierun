using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;


public class Application : Resource
{
    [JsonConstructor]
    protected Application(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }
    
    public Application(IHandler handler) : this(new Lazy<IHandler>(handler))
    {
    }

    public required string Name { get; init; }
    public Package? Package { get; init; }
    public string? Url { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<Resource> Resources { get; init; } = Array.Empty<Resource>();
    public IReadOnlyList<string> RequiredApplications { get; init; } = Array.Empty<string>();
}