using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class DockerVolume : Resource
{
    [JsonConstructor]
    protected DockerVolume(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public DockerVolume(IHandler handler) : base(handler)
    {
    }
    
    public required string Name { get; init; }
}
