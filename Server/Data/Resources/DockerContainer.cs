using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class DockerContainer : Resource
{
    [JsonConstructor]
    protected DockerContainer(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public DockerContainer(IHandler handler) : this(new Lazy<IHandler>(handler))
    {
    }

    public required string Name { get; init; }
    public required string NetworkName { get; init; }
}