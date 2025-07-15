using System.Diagnostics;
using Frierun.Server.Data;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Frierun.Server.Handlers.Udocker;

public class ContainerHandler(Application application)
    : Handler<Container>(application), IContainerHandler
{
    private readonly string _executablePath = application.Contracts.OfType<Parameter>().First(parameter => parameter.Name == "Path").Value ?? "";
    
    public override IEnumerable<ContractInitializeResult> Initialize(Container contract, string prefix)
    {
        yield return new ContractInitializeResult(
            contract with
            {
                ContainerName = contract.ContainerName ?? FindUniqueName(
                    prefix + (contract.Name == "" ? "" : $"-{contract.Name}"),
                    c => c.ContainerName
                ),
                DependsOn = contract.DependsOn.Append(contract.Network)
                    .Concat(contract.Mounts.Values.Select(mount => mount.Volume)),
                Handler = this
            }
        );
    }

    public override Container Install(Container contract, ExecutionPlan plan)
    {
        Debug.Assert(contract.ContainerName != null);
        Debug.Assert(contract.ImageName != null);

        var args = new List<string>();
        args.Add("run");
        args.Add("-d"); // not supported by udocker
        args.Add("--name");
        args.Add(contract.ContainerName);
        args.Add("--pull");
        args.Add("always");
        
        // mounts
        foreach (var (path, mount) in contract.Mounts)
        {
            var volume = plan.GetContract(mount.Volume);
            Debug.Assert(volume.Installed);
            Debug.Assert(volume.LocalPath != null);
            
            args.Add("--volume");
            args.Add($"{volume.LocalPath}:{path}");
        }
        
        // exposes ports
        var endpoints = plan.Contracts.OfType<PortEndpoint>().Where(ep => ep.Container == contract);
        foreach (var endpoint in endpoints)
        {
            Debug.Assert(endpoint.Installed);

            args.Add("--publish");
            args.Add($"{endpoint.ExternalPort}:{endpoint.Port}");
        }

        args.Add(contract.ImageName);

        var startInfo = new ProcessStartInfo(_executablePath, args);
        var process = Process.Start(startInfo);
        if (process == null)
        {
            throw new Exception("Failed to start process");
        }
        process.WaitForExit();
        
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

    public Task<(string stdout, string stderr)> ExecInContainer(Container container, IList<string> command)
    {
        throw new NotImplementedException();
    }
}