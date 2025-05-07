using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class DockerAttachedNetwork : Resource
{
    [JsonConstructor]
    protected DockerAttachedNetwork(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }
    
    public DockerAttachedNetwork(IHandler handler) : this(new Lazy<IHandler>(handler))
    {
    }
    
    public required string ContainerName { get; init; }
    public required  string NetworkName { get; init; }
}
