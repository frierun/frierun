using Frierun.Server.Data;
using Network = Frierun.Server.Data.Network;

namespace Frierun.Server.Installers.Docker;

public class NetworkInstaller(Application application, DockerService dockerService, State state)
    : IInstaller<Network>, IHandler<DockerNetwork>
{
    public Application Application => application;

    IEnumerable<InstallerInitializeResult> IInstaller<Network>.Initialize(Network contract, string prefix)
    {
        var baseName = contract.NetworkName ?? prefix + (contract.Name == "" ? "" : $"-{contract.Name}");

        var count = 1;
        var name = baseName;
        while (state.Resources.OfType<DockerNetwork>().Any(c => c.Name == name))
        {
            count++;
            name = $"{baseName}{count}";
        }

        yield return new InstallerInitializeResult(
            contract with
            {
                NetworkName = name
            }
        );
    }

    Resource IInstaller<Network>.Install(Network contract, ExecutionPlan plan)
    {
        var networkName = contract.NetworkName!;

        dockerService.CreateNetwork(networkName).Wait();

        return new DockerNetwork(this) { Name = networkName };
    }

    void IHandler<DockerNetwork>.Uninstall(DockerNetwork resource)
    {
        dockerService.RemoveNetwork(resource.Name).Wait();
    }
}