using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Frierun.Server.Handlers;
using static Frierun.Server.Data.Merger;

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
    IReadOnlyDictionary<string, string>? Labels = null,
    IReadOnlyDictionary<string, ContainerMount>? Mounts = null
) : Contract<IContainerHandler>(Name ?? ""), IHasStrings
{
    [MemberNotNullWhen(true, nameof(ContainerName), nameof(NetworkName))]
    public override bool Installed { get; init; }
    
    public IReadOnlyList<string> Command { get; init; } = Command ?? [];
    public IReadOnlyDictionary<string, string> Env { get; init; } = Env ?? new Dictionary<string, string>();
    public IReadOnlyDictionary<string, string> Labels { get; init; } = Labels ?? new Dictionary<string, string>();
    public IReadOnlyDictionary<string, ContainerMount> Mounts { get; init; } = Mounts ?? new Dictionary<string, ContainerMount>();
    
    [JsonInclude]
    private IDictionary<string, int> ConnectedNetworks { get; init; } = new Dictionary<string, int>();

    
    public ContractId<Network> Network { get; init; } = Network ?? new ContractId<Network>("");
    
    Contract IHasStrings.ApplyStringDecorator(Func<string, string> decorator)
    {
        return this with
        {
            Command = Command.Select(decorator).ToList(),
            Env = Env.ToDictionary(kv => decorator(kv.Key), kv => decorator(kv.Value))
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

    public override Contract Merge(Contract other)
    {
        var contract = EnsureSame(this, other);

        return MergeCommon(this, other) with
        {
            ContainerName = OnlyOne(ContainerName, contract.ContainerName),
            NetworkName = OnlyOne(NetworkName, contract.NetworkName),
            ImageName = OnlyOne(ImageName, contract.ImageName),
            RequireDocker = RequireDocker || contract.RequireDocker,
            Network = OnlyOne(Network, contract.Network),
            Command = OnlyOne(Command, contract.Command, command => command.Count == 0),
            Env = MergeDictionaries(Env, contract.Env),
            Labels = MergeDictionaries(Labels, contract.Labels),
            Mounts = MergeDictionaries(Mounts, contract.Mounts)
        };
    }
}