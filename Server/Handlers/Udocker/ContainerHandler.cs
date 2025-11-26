using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Udocker;

public class ContainerHandler(State state, Application application)
    : Handler<Container>(state, application), IContainerHandler
{
    private readonly SshConnection _connection = state.GetContract<SshConnection>(application);

    public override IEnumerable<ContractList> Initialize(Container contract, ApplicationContext context)
    {
        if (contract.MountDockerSocket == true)
        {
            yield break;
        }

        var containerRef = new ContractRef<Container>(context.Name);
        yield return new ContractList(
            contract.Mounts.Values.Select(mount => new KeyValuePair<ContractRef, Contract>(
                    mount.Volume.TypedRef,
                    new Volume { HandlerApplication = Application?.Name }
                )
            )
        )
        {
            [context] = contract with
            {
                ContainerName = contract.ContainerName ?? FindUniqueName(
                    context.Prefix + (context.Name == "" ? "" : $"-{context.Name}"),
                    c => c.ContainerName
                ),
                Handler = this,
                DependsOn =
                [
                    new ContractRef<Daemon>(context.Name)
                ]
            },
            [context.Name] = new Daemon(context.Name)
            {
                HandlerApplication = Application?.Name,
                Command = new Argument<IEnumerable<string>>(
                    new ArgumentResolver<ContractRef<Container>, IEnumerable<string>>(
                        containerRef,
                        GetCommands
                    )
                ),
                PreCommands = new Argument<IEnumerable<IEnumerable<string>>>(
                    new ArgumentResolver<ContractRef<Container>, IEnumerable<IEnumerable<string>>>(
                        containerRef,
                        GetPreCommands
                    )
                ),
                DependsOn = [
                    ..contract.Mounts.Values.Select(mount => mount.Volume)
                ]
            },
            [contract.Network.TypedRef] = new Network { HandlerApplication = Application?.Name }
        };
    }

    /// <summary>
    /// Gets udocker commands for the daemon
    /// </summary>
    private static IEnumerable<string> GetCommands(ContractRef<Container> contractRef, ExecutionPlan plan)
    {
        var contract = plan.GetContract(contractRef);
        Debug.Assert(contract.ContainerName != null);

        var command = new List<string>();
        command.Add("udocker");
        command.Add("run");

        // mounts
        foreach (var (path, mount) in contract.Mounts)
        {
            var volume = plan.GetContract(mount.Volume);
            Debug.Assert(volume.Installed);
            Debug.Assert(volume.LocalPath != null);

            command.Add($"--volume={volume.LocalPath}:{path}");
        }

        // exposes ports
        foreach (var port in contract.Ports)
        {
            command.Add($"--publish={port.ExternalPort}:{port.InternalPort}");
        }

        // envs
        foreach (var pair in contract.Env)
        {
            command.Add($"--env={pair.Key}={pair.Value.Resolve(plan).Value}");
        }

        command.Add(contract.ContainerName);

        return command;
    }

    /// <summary>
    /// Gets preparation commands to run udocker via daemon
    /// </summary>
    private static IEnumerable<IEnumerable<string>> GetPreCommands(ContractRef<Container> contractRef, ExecutionPlan plan)
    {
        var contract = plan.GetContract(contractRef);

        var imageName = contract.ImageName.Resolve(plan).Value;
        Debug.Assert(contract.ContainerName != null);
        Debug.Assert(imageName != null);

        var preCommands = new List<IEnumerable<string>>();
        preCommands.Add(["udocker", "rm", contract.ContainerName]);
        preCommands.Add(["udocker", "pull", imageName]);

        // create a container explicitly, otherwise it would spawn dangling containers
        preCommands.Add(
            ["udocker", "create", $"--name={contract.ContainerName}", imageName]
        );

        // mounts
        foreach (var (_, mount) in contract.Mounts)
        {
            var volume = plan.GetContract(mount.Volume);
            Debug.Assert(volume.Installed);
            Debug.Assert(volume.LocalPath != null);

            preCommands.Add(["mkdir", "-p", volume.LocalPath]);
        }

        return preCommands;
    }

    public override Container Install(Container contract, ExecutionPlan plan)
    {
        return contract with { NetworkName = "udocker" };
    }

    public void AttachNetwork(Container container, string networkName)
    {
        if (container.NetworkName != networkName)
        {
            throw new InvalidOperationException("Cannot attach to network");
        }
    }

    public void DetachNetwork(Container container, string networkName)
    {
    }

    public (string stdout, string stderr) ExecInContainer(Container container, IList<string> command)
    {
        Debug.Assert(container.Installed);

        // TODO we should mount same volumes
        var runCommand = new List<string>
        {
            "udocker",
            "run",
            container.ContainerName
        };
        runCommand.AddRange(command);

        using var sshClient = _connection.CreateSshClient();
        using var sshCommand = sshClient.RunCommand(String.Join(' ', runCommand.Select(SshConnection.EscapeArgument)));
        return (sshCommand.Result, sshCommand.Error);
    }
}