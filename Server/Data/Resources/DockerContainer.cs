using System.Text.Json.Serialization;
using Frierun.Server.Installers;

namespace Frierun.Server.Data;

public class DockerContainer : Resource
{
    [JsonConstructor]
    protected DockerContainer(Lazy<IHandler> lazyHandler) : base(lazyHandler)
    {
    }

    public DockerContainer(IContainerHandler handler) : base(handler)
    {
    }

    /// <inheritdoc />
    [JsonIgnore]
    public override IContainerHandler Handler => (IContainerHandler)base.Handler;
    
    public void AttachNetwork(string networkName)
    {
        Handler.AttachNetwork(this, networkName);
    }
    
    public void DetachNetwork(string networkName)
    {
        Handler.DetachNetwork(this, networkName);
    }
    
    public Task<(string stdout, string stderr)> ExecInContainer(IList<string> command)
    {
        return Handler.ExecInContainer(this, command);
    }

    public required string Name { get; init; }
    public required string NetworkName { get; init; }
}