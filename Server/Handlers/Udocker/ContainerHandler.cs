using System.Diagnostics;
using Frierun.Server.Data;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Frierun.Server.Handlers.Udocker;

public class ContainerHandler(Application application)
    : Handler<Container>(application), IContainerHandler
{
    private readonly SshConnection _connection = application.Contracts.OfType<SshConnection>().Single();

    public override IEnumerable<ContractInitializeResult> Initialize(Container contract, string prefix)
    {
        if (contract.MountDockerSocket)
        {
            yield break;
        }

        yield return new ContractInitializeResult(
            contract with
            {
                ContainerName = contract.ContainerName ?? FindUniqueName(
                    prefix + (contract.Name == "" ? "" : $"-{contract.Name}"),
                    c => c.ContainerName
                ),
                Handler = this
            },
            [
                new Daemon(contract.Name)
                {
                    HandlerApplication = Application?.Name,
                    DependsOn = [contract]
                },
                new Network(contract.Network.Name)
                {
                    HandlerApplication = Application?.Name,
                    DependencyOf = [contract]
                },
                ..contract.Mounts.Values.Select(mount => new Volume(mount.Volume.Name)
                    {
                        HandlerApplication = Application?.Name,
                        DependencyOf = [contract]
                    }
                )
            ]
        );
    }

    public override Container Install(Container contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.ContainerName != null);
        Debug.Assert(contract.ImageName != null);

        var preCommands = new List<IReadOnlyList<string>>();
        preCommands.Add(new List<string> { "udocker", "rm", contract.ContainerName });
        preCommands.Add(new List<string> { "udocker", "pull", contract.ImageName });

        // create container explicitly, otherwise it would spawn dangling containers
        preCommands.Add(
            new List<string> { "udocker", "create", $"--name={contract.ContainerName}", contract.ImageName }
        );

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
            preCommands.Add(new List<string> { "mkdir", "-p", volume.LocalPath });
        }

        // exposes ports
        var endpoints = plan.Contracts.OfType<PortEndpoint>().Where(ep => ep.Container == contract);
        foreach (var endpoint in endpoints)
        {
            Debug.Assert(endpoint.Installed);

            command.Add($"--publish={endpoint.ExternalPort}:{endpoint.Port}");
        }

        // envs
        foreach (var pair in contract.Env)
        {
            command.Add($"--env={pair.Key}={pair.Value}");
        }

        command.Add(contract.ContainerName);

        var daemon = plan.GetContract(new ContractId<Daemon>(contract.Name));
        plan.ReplaceContract(
            daemon with
            {
                Command = command,
                PreCommands = preCommands
            }
        );

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