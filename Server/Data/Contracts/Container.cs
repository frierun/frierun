using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Docker.DotNet.Models;
using Frierun.Server.Handlers;

namespace Frierun.Server.Data;

public record Container(
    string? Name = null,
    string? ContainerName = null,
    string? NetworkName = null,
    string? ImageName = null,
    bool RequireDocker = false,
    ContractId<Network>? Network = null,
    IReadOnlyList<string>? Command = null,
    IReadOnlyDictionary<string, string>? Env = null,
    IEnumerable<Action<CreateContainerParameters>>? Configure = null
) : Contract(Name ?? ""), IHasStrings
{
    [MemberNotNullWhen(true, nameof(ContainerName), nameof(NetworkName))]
    public override bool Installed { get; init; }

    [JsonIgnore]
    public new IContainerHandler? Handler
    {
        get => (IContainerHandler?)LazyHandler.Value;
        init => LazyHandler = new Lazy<IHandler?>(value);
    }
    
    public IReadOnlyList<string> Command { get; init; } = Command ?? Array.Empty<string>();
    public IReadOnlyDictionary<string, string> Env { get; init; } = Env ?? new Dictionary<string, string>();
    
    [JsonInclude]
    private IDictionary<string, int> ConnectedNetworks { get; init; } = new Dictionary<string, int>();

    [JsonIgnore]
    public IEnumerable<Action<CreateContainerParameters>> Configure { get; init; } =
        Configure ?? Array.Empty<Action<CreateContainerParameters>>();
    
    
    public ContractId<Network> Network { get; init; } = Network ?? new ContractId<Network>("");
    
    Contract IHasStrings.ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Command = Command.Select(decorator).ToList(),
            Env = Env.ToDictionary(kv => decorator(kv.Key), kv => decorator(kv.Value)),
        };
    }
    
    /// <summary>
    /// Attaches the container to a network.
    /// </summary>
    public void AttachNetwork(string networkName)
    {
        Debug.Assert(Handler != null);
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
    
    /// <summary>
    /// Detaches container from a network.
    /// </summary>
    public void DetachNetwork(string networkName)
    {
        Debug.Assert(Handler != null);
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
    
    /// <summary>
    /// Executes a command in the container.
    /// </summary>
    public Task<(string stdout, string stderr)> ExecInContainer(IList<string> command)
    {
        Debug.Assert(Handler != null);
        return Handler.ExecInContainer(this, command);
    }    
}