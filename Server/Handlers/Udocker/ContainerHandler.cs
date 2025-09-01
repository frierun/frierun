using System.Diagnostics;
using Frierun.Server.Data;

namespace Frierun.Server.Handlers.Udocker;

public class ContainerHandler(Application application)
    : Handler<Container>(application), IContainerHandler
{
    private readonly SshConnection _connection = application.GetContract(new ContractId<SshConnection>());

    public override IEnumerable<ContractList> Initialize(Container contract, ApplicationContext context)
    {
        if (contract.MountDockerSocket)
        {
            yield break;
        }

        var contractId = contract.Id;
        yield return new ContractList(
            contract.Mounts.Values.Select(mount => new Volume(mount.Volume.Name)
                {
                    HandlerApplication = Application?.Name
                }
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
                    contract.Network,
                    new ContractId<Daemon>(context.Name),
                    ..contract.Mounts.Values.Select(mount => mount.Volume)
                ]
            },
            [context.Name] = new Daemon(context.Name)
            {
                HandlerApplication = Application?.Name,
                Command = new Argument<IEnumerable<string>>(
                    new ArgumentResolver<ContractId, IEnumerable<string>>(
                        contractId,
                        GetCommands
                    )
                ),
                PreCommands = new Argument<IEnumerable<IEnumerable<string>>>(
                    new ArgumentResolver<ContractId, IEnumerable<IEnumerable<string>>>(
                        contractId,
                        GetPreCommands
                    )
                ),
                DependsOn = [..contract.Mounts.Values.Select(mount => mount.Volume)]
            },
            [contract.Network] = new Network(contract.Network.Name)
            {
                HandlerApplication = Application?.Name
            }
        };
    }

    /// <summary>
    /// Gets udocker commands for the daemon
    /// </summary>
    private static IEnumerable<string> GetCommands(ContractId contractId, ExecutionPlan plan)
    {
        var contract = (Container)plan.GetContract(contractId);
        foreach (var argument in contract.GetArguments())
        {
            argument.Resolve(plan);
        }

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
        var endpoints = plan.Contracts.OfType<PortEndpoint>().Where(ep => ep.Container == contract.Id);
        foreach (var endpoint in endpoints)
        {
            command.Add($"--publish={endpoint.ExternalPort}:{endpoint.Port}");
        }

        // envs
        foreach (var pair in contract.Env)
        {
            command.Add($"--env={pair.Key}={pair.Value.Value}");
        }

        command.Add(contract.ContainerName);

        return command;
    }

    /// <summary>
    /// Gets preparation commands to run udocker via daemon
    /// </summary>
    private static IEnumerable<IEnumerable<string>> GetPreCommands(ContractId contractId, ExecutionPlan plan)
    {
        var contract = (Container)plan.GetContract(contractId);
        foreach (var argument in contract.GetArguments())
        {
            argument.Resolve(plan);
        }

        var imageName = contract.ImageName.Value;
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
        foreach (var (path, mount) in contract.Mounts)
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