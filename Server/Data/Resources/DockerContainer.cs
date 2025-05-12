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

    [JsonIgnore]
    public override IContainerHandler Handler => (IContainerHandler)base.Handler;

    [JsonInclude]
    private IDictionary<string, int> ConnectedNetworks { get; init; } = new Dictionary<string, int>();
    public required string Name { get; init; }
    public required string NetworkName { get; init; }
    
    public void AttachNetwork(string networkName)
    {
        if (networkName == NetworkName)
        {
            return;
        }
        
        if (ConnectedNetworks.TryGetValue(networkName, out var count))
        {
            ConnectedNetworks[networkName] = count + 1;
        }
        else
        {
            ConnectedNetworks[networkName] = 1;
            Handler.AttachNetwork(this, networkName);
        }
    }
    
    public void DetachNetwork(string networkName)
    {
        if (networkName == NetworkName)
        {
            return;
        }
        
        if (ConnectedNetworks.TryGetValue(networkName, out var count))
        {
            if (count > 1)
            {
                ConnectedNetworks[networkName] = count - 1;
                return;
            }

            ConnectedNetworks.Remove(networkName);
        }

        Handler.DetachNetwork(this, networkName);
    }
    
    public Task<(string stdout, string stderr)> ExecInContainer(IList<string> command)
    {
        return Handler.ExecInContainer(this, command);
    }
}