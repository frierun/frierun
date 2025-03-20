using Frierun.Server.Data;

namespace Frierun.Server.Installers.Docker;

public class AttachNetworkInstaller(DockerService dockerService, State state)
    : IInstaller<ConnectExternalContainer>, IUninstaller<DockerAttachedNetwork>
{
    /// <inheritdoc />
    IEnumerable<InstallerInitializeResult> IInstaller<ConnectExternalContainer>.Initialize(
        ConnectExternalContainer contract,
        string prefix
    )
    {
        yield return new InstallerInitializeResult(
            contract,
            [contract.NetworkId]
        );
    }

    /// <inheritdoc />
    IEnumerable<ContractDependency> IInstaller<ConnectExternalContainer>.GetDependencies(
        ConnectExternalContainer contract,
        ExecutionPlan plan
    )
    {
        yield return new ContractDependency(contract.NetworkId, contract.Id);
    }

    /// <inheritdoc />
    Resource IInstaller<ConnectExternalContainer>.Install(ConnectExternalContainer contract, ExecutionPlan plan)
    {
        var network = plan.GetResource<DockerNetwork>(contract.NetworkId);

        var container = state.Resources
            .OfType<DockerContainer>()
            .First(resource => resource.Name == contract.ContainerName);

        var alreadyAttached = state.Resources
            .OfType<DockerAttachedNetwork>()
            .Any(
                resource => resource.ContainerName == contract.ContainerName &&
                            resource.NetworkName == contract.NetworkName
            );

        if (container.NetworkName != network.Name && !alreadyAttached)
        {
            dockerService.AttachNetwork(network.Name, contract.ContainerName).Wait();
        }

        return new DockerAttachedNetwork(ContainerName: contract.ContainerName, NetworkName: network.Name)
        {
            DependsOn = [network]
        };
    }

    /// <inheritdoc />
    public void Uninstall(DockerAttachedNetwork attachedNetwork)
    {
        var container = state.Resources
            .OfType<DockerContainer>()
            .First(resource => resource.Name == attachedNetwork.ContainerName);

        if (container.NetworkName == attachedNetwork.NetworkName)
        {
            return;
        }

        var alreadyAttached = state.Resources
            .OfType<DockerAttachedNetwork>()
            .Count(
                resource => resource.ContainerName == attachedNetwork.ContainerName &&
                            resource.NetworkName == attachedNetwork.NetworkName
            );

        if (alreadyAttached > 1)
        {
            return;
        }

        dockerService.DetachNetwork(attachedNetwork.NetworkName, attachedNetwork.ContainerName).Wait();
    }
}