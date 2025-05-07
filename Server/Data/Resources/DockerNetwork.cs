using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class DockerNetwork : Resource
{
    [JsonConstructor]
    protected DockerNetwork(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public DockerNetwork(IHandler handler) : this(new Lazy<IHandler>(handler))
    {
    }
    
    public required string Name { get; init; }
}