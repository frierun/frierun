using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Frierun.Server.Handlers;
using static Frierun.Server.Data.Merger;

namespace Frierun.Server.Data;

public record Container(
    string? ContainerName = null,
    string? NetworkName = null,
    Argument<string>? ImageName = null,
    bool? MountDockerSocket = null,
    ContractRef<Network>? Network = null,
    IEnumerable<ContainerPort>? Ports = null,
    Argument<IEnumerable<string>>? Command = null,
    IEnumerable<string>? NetworkAliases = null,
    IReadOnlyDictionary<string, Argument<string>>? Env = null,
    IReadOnlyDictionary<string, Argument<string>>? Labels = null,
    IReadOnlyDictionary<string, ContainerMount>? Mounts = null
) : Contract<IContainerHandler>
{
    [MemberNotNullWhen(true, nameof(ContainerName), nameof(NetworkName))]
    public override bool Installed => Id != Guid.Empty;
    
    public Argument<IEnumerable<string>> Command { get; init; } = Command ?? new Argument<IEnumerable<string>>();
    public IReadOnlyDictionary<string, Argument<string>> Env { get; init; } = Env ?? new Dictionary<string, Argument<string>>();
    public IReadOnlyDictionary<string, Argument<string>> Labels { get; init; } = Labels ?? new Dictionary<string, Argument<string>>();
    public IReadOnlyDictionary<string, ContainerMount> Mounts { get; init; } = Mounts ?? new Dictionary<string, ContainerMount>();
    public IEnumerable<ContainerPort> Ports { get; init; } = Ports ?? [];
    public ContractRef<Network> Network { get; init; } = Network ?? new ContractRef<Network>("");
    public IEnumerable<string> NetworkAliases { get; init; } = NetworkAliases ?? [];
    public Argument<string> ImageName { get; init; } = ImageName ?? new Argument<string>();
   
    
    [JsonInclude]
    private IDictionary<string, int> ConnectedNetworks { get; init; } = new Dictionary<string, int>();

    public override IEnumerable<IArgument> GetArguments()
    {
        yield return ImageName;
        yield return Command;
        foreach (var pair in Env)
        {
            yield return pair.Value;
        }
        foreach (var pair in Labels)
        {
            yield return pair.Value;
        }
        foreach (var argument in base.GetArguments())
        {
            yield return argument;
        }
    }
    
    public override Contract Merge(Contract other)
    {
        return MergeCommon(this, other, out var contract) with
        {
            ContainerName = MergeValue(ContainerName, contract.ContainerName),
            NetworkName = MergeValue(NetworkName, contract.NetworkName),
            ImageName = ImageName.Merge(contract.ImageName),
            MountDockerSocket = MergeValue(MountDockerSocket, contract.MountDockerSocket),
            Network = MergeValue(Network, contract.Network),
            Ports = Ports.Concat(contract.Ports).Distinct(),
            Command = Command.Merge(contract.Command),
            NetworkAliases = NetworkAliases.Concat(contract.NetworkAliases).Distinct(),
            Env = MergeDictionary(Env, contract.Env),
            Labels = MergeDictionary(Labels, contract.Labels),
            Mounts = MergeDictionary(Mounts, contract.Mounts)
        };
    }

    public override bool IsSubset(Contract other)
    {
        return IsSubsetContract(this, other, out var contract)
            && IsSubsetValue(ContainerName, contract.ContainerName)
            && IsSubsetValue(NetworkName, contract.NetworkName)
            && IsSubsetArgument(ImageName, contract.ImageName)
            && IsSubsetValue(MountDockerSocket, contract.MountDockerSocket)
            && IsSubsetValue(Network, contract.Network)
            && IsSubsetList(Ports, contract.Ports)
            && IsSubsetArgument(Command, contract.Command)
            && IsSubsetList(NetworkAliases, contract.NetworkAliases)
            && IsSubsetDictionary(Env, contract.Env)
            && IsSubsetDictionary(Labels, contract.Labels)
            && IsSubsetDictionary(Mounts, contract.Mounts);
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
    public (string stdout, string stderr) ExecInContainer(IList<string> command)
    {
        Debug.Assert(Handler != null);
        return Handler.ExecInContainer(this, command);
    }
}